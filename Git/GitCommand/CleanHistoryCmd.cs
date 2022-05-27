using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand
    {
        public static void CleanHistoryCmd(bool all, List<string> paths)
        {
            paths = paths.Select(path => Path.GetFullPath(path)).ToList();

            // valid non-bare repo
            GitFS gitfs = new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr != null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);
            paths = paths.Select(path => gitfs.gitp.RelToRoot(path)).ToList();

            string new_commit_hash = ModifyCommit(gitfs.head.Hash, all ? null : paths);
            gitfs.head.SetHead(new_commit_hash);
            // tmp
            gitfs.tmp.RemoveEntries(all ? null : paths);
            gitfs.tmp.WriteTmp();
            // track
            gitfs.track.RemoveEntries(all ? null : paths);
            gitfs.track.WriteTrack();

            Console.WriteLine($"★ {new_commit_hash}");
            Console.WriteLine($"temporary files were successfully deleted");

            string ModifyCommit(string commit_hash, List<string> paths = null)
            {
                var commit = new Commit(gitfs, commit_hash);
                var tree = new Tree(gitfs, commit.Content.tree_hash);

                var i = tree.Entries.FindIndex(te => te.name == ".tmp");
                if (i == -1)
                    return commit_hash;
                var te = tree.Entries[i];
                var tmp = new Tmp(gitfs, te.hash);
                tmp.RemoveEntries(paths);
                tree.Entries[i] = new TreeEntry
                {
                    mode = te.mode,
                    name = te.name,
                    hash = tmp.WriteLikeBlob()
                };

                i = tree.Entries.FindIndex(te => te.name == ".track");
                te = tree.Entries[i];
                var track = new Track(gitfs, te.hash);
                track.RemoveEntries(paths);
                tree.Entries[i] = new TreeEntry
                {
                    mode = te.mode,
                    name = te.name,
                    hash = track.WriteLikeBlob()
                };

                commit.Content.tree_hash = tree.WriteTree();
                List<string> new_parent_hashes = new List<string>();
                foreach (var phash in commit.Content.parent_hashes)
                {
                    new_parent_hashes.Add(ModifyCommit(phash, paths));
                }
                commit.Content.parent_hashes = new_parent_hashes;
                return commit.WriteCommit();
            }
        }
    }
}