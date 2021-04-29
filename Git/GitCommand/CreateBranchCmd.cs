using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void CreateBranchCmd(string branch_name)
        {
            
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            if (branch_name==null)
            {
                string cur_branch=gitfs.head.Branch;
                foreach(var branch in gitfs.Refs.Where(iref=>iref.Key.StartsWith("heads")).Select(iref=>iref.Key.Substring(6)))
                {
                    string pref = branch==cur_branch?"* ":"  ";
                    Console.WriteLine($"{pref}{branch}");
                }
            }
            else if (gitfs.head.Branch==null)
            {
                throw new Exception($"{gitfs.head.Content} is not valid object");
            }
            else if (gitfs.head.Branch!="master")
            {
                throw new Exception($"branching from non-master branch is forbidden");
            }
            else if (gitfs.Refs.ContainsKey($"heads/{branch_name}"))
            {
               throw new Exception($"{branch_name} already exists"); 
            }
            else
            {
                string ref_name = $"heads/{branch_name}";
                gitfs.Refs[ref_name]=new Ref(gitfs,ref_name,false);
                gitfs.Refs[ref_name].SetRef(gitfs.head.Hash);
                Console.WriteLine($"{branch_name} branch created");
            }
        }
    }
}