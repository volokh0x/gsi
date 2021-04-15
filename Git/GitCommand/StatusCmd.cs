using System;
using System.IO;

namespace gsi
{
    static partial class GitCommand 
    {
        public static void SatusCmd()
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            (var lch, var lnew, var ldel)=gitfs.index.GetStatus();
            if (lch.Count!=0)
            {
                Console.WriteLine("Changed files:");
                foreach (var f in lch) Console.WriteLine($"\t{f}");
                Console.WriteLine();
            }
            if (lnew.Count!=0)
            {
                Console.WriteLine("New files:");
                foreach (var f in lnew) Console.WriteLine($"\t{f}");
                Console.WriteLine();
            }
            if (ldel.Count!=0)
            {
                Console.WriteLine("Deleted files:");
                foreach (var f in ldel) Console.WriteLine($"\t{f}");
                Console.WriteLine();
            }
        }
    }
}