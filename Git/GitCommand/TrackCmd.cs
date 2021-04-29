using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void TrackCmd(List<string> paths)
        {
            paths=paths.Select(path => Path.GetFullPath(path)).ToList();
            
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            // user files, if dir then recursive
            var fpaths = gitfs.GetFiles(paths);
            if (fpaths.Count==0)
            {
                Console.WriteLine("nothing specified, nothing was marked as tracked");
                return;
            }
            
            if (gitfs.track==null)
                gitfs.track=new Track(gitfs,false);
            foreach(var path in fpaths)
            {
                string relpath=gitfs.gitp.RelToRoot(path);
                gitfs.track.SetEntry(relpath,true);
                Console.WriteLine($"{relpath} is tracked");
            } 
            gitfs.track.WriteTrack();      
        }
    }
}