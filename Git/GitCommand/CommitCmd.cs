using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void CommitCmd(string message)
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);
            
            // read index 
            if (gitfs.index==null)
                throw new Exception("index file does not exist");
            
            string tree_hash = gitfs.WriteTreeGraph();
            string head_desc=gitfs.head.IsDetached?"detached HEAD":gitfs.head.Content;

            if (tree_hash==gitfs.head.Hash)
                throw new Exception($"On {head_desc} nothing to commit, working directory clean");

            var ies=gitfs.index.GetConfilctingEntries();
            if (gitfs.merge_head!=null && ies.Count>0)
                throw new Exception($"{string.Join("\n",ies.Select(ie=>ie.path))}\ncannot commit because you have unmerged files\n");

            string msg=gitfs.merge_head!=null?gitfs.merge_msg.Content:message;
            
            string commit_hash = Commit.AddCommit(gitfs,tree_hash,msg);
            gitfs.head.SetHead(commit_hash);

            if (gitfs.merge_head!=null)
            {
                gitfs.merge_head.Delete();
                gitfs.merge_msg.Delete();
                Console.WriteLine("Merge made by the three-way strategy");
            }
            else
            {
                Console.WriteLine($"[{head_desc} {commit_hash}] {msg}");
            }
        }
    }
}