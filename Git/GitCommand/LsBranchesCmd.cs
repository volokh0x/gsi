using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void LsBranchesCmd()
        {
            
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            string cur_branch=gitfs.head.Branch;
            foreach(var branch in gitfs.Refs.Where(iref=>iref.Key.StartsWith("heads")).Select(iref=>iref.Key.Substring(6)))
            {
                string pref = branch==cur_branch?"* ":"  ";
                Console.WriteLine($"{pref}{branch}");
            }
        }
    }
}