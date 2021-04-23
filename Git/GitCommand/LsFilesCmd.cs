using System;
using System.IO;
using System.Collections.Generic;

namespace gsi
{
    static partial class GitCommand 
    {
        public static void LsFilesCmd()
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            if (gitfs.index.Entries.Count==0)
            {
                Console.WriteLine("staging area is empty");
                return;
            }
            foreach(var ie in gitfs.index.Entries)
                Console.WriteLine(ie);
        }
    }
}