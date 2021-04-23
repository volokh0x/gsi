using System;
using System.IO;
using System.Collections.Generic;

namespace gsi
{
    static partial class GitCommand 
    {
        public static void SatusCmd()
        {
            bool PF(string msg, List<string> L)
            {
                if (L.Count!=0)
                {
                    Console.WriteLine(msg);
                    foreach (var f in L) Console.WriteLine($"\t{f}");
                    return true;
                }
                return false;
            }
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            (var lch, var lnew, var ldel)=gitfs.index.GetStatus();
            bool done=false;
            done=done || PF("Changed files:",lch);
            done=done || PF("New files:",lnew);
            done=done || PF("Deleted files:",ldel);
            if (done) return; 
            Console.WriteLine("working tree is clean");
        }
    }
}