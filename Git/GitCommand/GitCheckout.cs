using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void CheckoutCmd(string ref_or_hash)
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);
            (bool detached, string hash) = Ref.GetIsDetachedAndHash(gitfs,ref_or_hash);
            if (!File.Exists(gitfs.gitp.PathFromHash(hash)))
                throw new Exception($"{hash} now found");
            Commit c = new Commit(gitfs,hash);
            if (ref_or_hash==gitfs.head.Content) 
                throw new Exception($"already on {ref_or_hash}");

            var diffs = DiffCalc.Diff(gitfs,gitfs.head.Hash, hash);
            foreach(var el in diffs)
            {
                Console.WriteLine($"{el.Key} {el.Value}");
            }
            //gitfs.ApplyDiff(diffs);


        }
    }
}