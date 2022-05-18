using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand
    {
        public static void CommitCmd(string message,List<string> included,List<string> excluded)
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            included = gitfs.GetFiles(included);
            excluded = gitfs.GetFiles(excluded);

            // read index and track
            if (gitfs.index==null)
                gitfs.index=new Index(gitfs,false);
            if (gitfs.track==null)
                gitfs.track=new Track(gitfs,false);

            (var lmer,var lch, var lnew, var ldel)=gitfs.TrackWorkingCopy();
            gitfs.track.WriteTrack();

            gitfs.track.SetEntries(included,Track.Status.INCLUDED);
            gitfs.track.SetEntries(excluded,Track.Status.EXLUDED);
            (lmer, lch, lnew, ldel)=gitfs.TrackWorkingCopy();
            var add = lch.Union(lnew).Append(".track");
            var del = ldel;

            // .track file inside add, path does not exists
            foreach(var path in add)
            {
                Blob blob=new Blob(gitfs,path,true);
                string hash = blob.WriteBlob();
                gitfs.index.AddEntry(path, hash);
            }
            foreach(var path in del)
            {
                gitfs.index.DelEntry(path);
            }
            gitfs.index.WriteIndex();

            string tree_hash = gitfs.WriteTreeGraph();
            string head_tree_hash = gitfs.head.Hash!=null?new Commit(gitfs,gitfs.head.Hash).Content.tree_hash:null;
            string head_desc=gitfs.head.Branch;

            if (tree_hash==head_tree_hash && gitfs.merge_head==null)
                throw new Exception($"on {head_desc} nothing to commit, working directory clean");

            if (lmer.Count!=0)
            {
                Console.WriteLine("you are in the middle of a merge");
                Console.WriteLine($"conflicting files were:");
                foreach(var path in lmer) Console.WriteLine($"! {path}");
                Repeat:
                Console.WriteLine("have you resolved these conflicts? [y/n]");
                var ans = Console.ReadLine();
                ans=ans.Trim();
                if (ans=="n" || ans=="N") return;
                if (!(ans=="y" || ans=="Y")) goto Repeat;
                foreach(var path in lmer)
                {
                    Blob blob=new Blob(gitfs,path,true);
                    string hash = blob.WriteBlob();
                    gitfs.index.AddEntry(path, hash);
                }
            }
            string msg;
            if (gitfs.merge_head!=null) msg=gitfs.merge_msg.Content.Split("\n")[0];
            else if (message!=null) msg=message;
            else throw new Exception("message's not specified");

            string commit_hash = gitfs.CreateCommit(gitfs,tree_hash,msg);
            gitfs.head.SetHead(commit_hash);

            if (gitfs.merge_head!=null)
            {
                gitfs.merge_head.Delete();
                gitfs.merge_msg.Delete();
                Console.WriteLine("merge made by the three-way strategy");
            }
            Console.WriteLine($"[{head_desc} {commit_hash}] {msg}");
        }
    }
}