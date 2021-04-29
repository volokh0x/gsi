using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace gsi
{
    static partial class GitCommand 
    {
        public static void SatusCmd()
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            bool PF(string msg, List<string> L)
            {
                var l=new List<string>();
                foreach(var path in L)
                {
                    if (gitfs.track.included.Contains(path)) 
                        l.Add(path);
                    else if (!gitfs.track.excluded.Contains(path))
                    {
                        gitfs.track.SetEntry(path,true);
                        l.Add(path);
                    }
                }
                if (l.Count!=0)
                {
                    Console.WriteLine(msg);
                    foreach (var f in l) Console.WriteLine($"\t{f}");
                    return true;
                }
                return false;
            }

            // read index and track
            if (gitfs.index==null)
                gitfs.index=new Index(gitfs,false);
            if (gitfs.track==null)
                gitfs.track=new Track(gitfs,false);
                
            (var lmer,var lch, var lnew, var ldel)=gitfs.TrackWorkingCopy();

            bool done1 = PF("Conflicting files:",lmer);
            bool done2 = PF("Changed files:",lch);
            bool done3 = PF("New files:",lnew);
            bool done4 = PF("Deleted files:",ldel);
            if (!(done1 || done2 || done3 || done4))
                Console.WriteLine("working tree is clean");
        }
    }
}