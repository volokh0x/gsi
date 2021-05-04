using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void BranchCmd(List<string> create, List<string> delete)
        {
            
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            if (create.Count+delete.Count==0)
            {
                string cur_branch=gitfs.head.Branch;
                foreach(var branch in gitfs.Refs.Where(iref=>iref.Key.StartsWith("heads")).Select(iref=>iref.Key.Substring(6)).OrderBy(path=>path))
                {
                    string pref = branch==cur_branch?"*":"🖼";
                    Console.WriteLine($"{pref} {gitfs.Refs[$"heads/{branch}"].Hash} {branch}");
                }
                return;
            }
            if (gitfs.head.Branch==null)
            {
                throw new Exception($"{gitfs.head.Content} is not valid object");
            }
            foreach(var branch_name in delete)
            {
                if (branch_name=="master")
                {
                    Console.WriteLine($"deleting master branch is forbidden");
                    continue;
                }
                if (gitfs.head.Branch==branch_name)
                {
                    Console.WriteLine($"deleting current branch is forbidden");
                    continue;
                }
                if (!gitfs.Refs.ContainsKey($"heads/{branch_name}"))
                {
                    Console.WriteLine($"✘ {branch_name} does not exist");
                    continue;
                }
                string ref_name = $"heads/{branch_name}";
                gitfs.Refs[ref_name]=new Ref(gitfs,ref_name,false);
                gitfs.Refs[ref_name].Delete();
                Console.WriteLine($"✔ {branch_name} branch deleted");
            }
            foreach(var branch_name in create)
            {
                if (gitfs.head.Branch!="master")
                {
                    Console.WriteLine($"branching from non-master branch is forbidden");
                    continue;
                }
                if (gitfs.Refs.ContainsKey($"heads/{branch_name}"))
                {
                    Console.WriteLine($"✘ {branch_name} already exists");
                    continue;
                }
                string ref_name = $"heads/{branch_name}";
                gitfs.Refs[ref_name]=new Ref(gitfs,ref_name,false);
                gitfs.Refs[ref_name].SetRef(gitfs.head.Hash);
                Console.WriteLine($"✔ {branch_name} branch created");
            }
        }
    }
}