using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void RestoreCmd(string path, string hash_prefix)
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            // read index and track
            if (gitfs.index==null)
                gitfs.index=new Index(gitfs,false);
            if (gitfs.track==null)
                gitfs.track=new Track(gitfs,false);

            var blob = new Blob(gitfs,gitfs.gitp.PathFromHash(hash_prefix),false);
            string pardir=Path.GetDirectoryName(path);
            if (pardir!=string.Empty)
                Directory.CreateDirectory(pardir);
            File.WriteAllText(path,blob.Content);
        }
    }
}