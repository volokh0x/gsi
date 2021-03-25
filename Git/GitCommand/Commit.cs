using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void Commit(string msg)
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            gitfs.config.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);
            
            // read index 
            if (gitfs.index==null)
                gitfs.index=new Index(gitfs.gitp.PathFromRoot("index"),true);
            
            string hash = Tree.WriteTreeGraph(gitfs);

            string head_desc=gitfs.head.ToBranch?gitfs.head.Content:"detached HEAD";
        }
        public static void F(string root, List<IndexEntry> ies)
        {

        }
    }
}