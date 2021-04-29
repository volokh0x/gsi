using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void SwitchCmd(string ref_or_hash)
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            (bool detached, string hash) = Ref.GetIsDetachedAndHash(gitfs,ref_or_hash);
            if (detached)
            {
                Console.WriteLine("Your current branch will be pointing to this commit");
                Console.WriteLine("Current branch history could be lost");
                L:
                Console.WriteLine("Continue? [y/n]");
                var ans = Console.ReadLine();
                ans=ans.Trim();
                if (ans=="n" || ans=="N") return;
                if (!(ans=="y" || ans=="Y")) goto L;
            }
            if ($"heads/{ref_or_hash}"==gitfs.head.Content) 
                throw new Exception($"already on {ref_or_hash}");
            gitfs.Objs[hash]=new Commit(gitfs,hash);

            var paths = DiffCalc.CommitWouldOverwrite(gitfs); 
            
            if (paths.Count!=0)
                throw new Exception($"local changes would be lost:\n\t {string.Join("\n\t",paths)}");
            
            gitfs.ApplyDiff(DiffCalc.Diff(gitfs,gitfs.head.Hash,hash),"HEAD",ref_or_hash); 
            if (detached) 
                gitfs.head.SetHead(hash,false);
            else 
                gitfs.head.SetHead(gitfs.Refs[$"heads/{ref_or_hash}"]); 
            gitfs.index.SetFromStorage(Num.GIVER); 
            gitfs.index.WriteIndex();
            if (detached)
                Console.WriteLine($"Note: switching to {hash}");
            else   
                Console.WriteLine($"Switched to branch {ref_or_hash}");
        }
    }
}