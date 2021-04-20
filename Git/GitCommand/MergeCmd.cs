using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void MergeCmd(string ref_or_hash)
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            (bool detached, string giver) = Ref.GetIsDetachedAndHash(gitfs,ref_or_hash);
            string receiver = gitfs.head.Hash;
            if (gitfs.head.IsDetached)
                throw new Exception("merge to detached HEAD is unsupported");
                
            gitfs.Objs[giver]=new Commit(gitfs,giver);
            string bbase = gitfs.GetBaseCommitHash(receiver,giver);
            if (bbase==giver)
            {
                Console.WriteLine("already up to date");
                return;
            }

            var paths = DiffCalc.CommitWouldOverwrite(gitfs);
            if (paths.Count!=0)
                throw new Exception($"local changes would be lost:\n\t {string.Join("\n\t",paths)}");

            if (bbase==receiver)
            {
                gitfs.ApplyDiff(DiffCalc.Diff(gitfs,receiver,giver)); 

                gitfs.index.SetFromStorage(Num.GIVER); 
                gitfs.index.WriteIndex();

                gitfs.head.SetHead(giver);
                Console.WriteLine($"fast-forward merge [{receiver}] => [{giver}]");
            }
            else
            {
                gitfs.merge_head=new MergeHead(gitfs,false);
                gitfs.merge_head.SetMergeHead(giver);
                
                var diffs = DiffCalc.Diff(gitfs,receiver,giver,bbase);
                var conflicts = diffs.Where(fds=>fds.Value==FileDiffStatus.CONFLICT).ToDictionary(p=>p.Key,p=>p.Value);

                gitfs.ApplyDiff(diffs);

                gitfs.index.SetFromStorage(Num.RECEIVER,Num.GIVER,Num.BASE,diffs); 
                gitfs.index.WriteIndex();

                gitfs.merge_msg = new MergeMsg(gitfs,false);
                gitfs.merge_msg.SetMergeMsg(ref_or_hash,conflicts);

                if (conflicts.Count()!=0)
                    throw new Exception("automatic merge failed. Fix conflicts and commit the result");

                CommitCmd(null);

            }
        }
    }
}