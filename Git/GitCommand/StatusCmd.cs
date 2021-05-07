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
                if (L.Count!=0)
                {
                    Console.WriteLine(msg);
                    foreach (var f in L) Console.WriteLine($"  {f}");
                    return true;
                }
                return false;
            }

            // read index and track
            if (gitfs.index==null)
                gitfs.index=new Index(gitfs,false);
            if (gitfs.track==null)
                gitfs.track=new Track(gitfs,false);
            Console.WriteLine($"🖼 {gitfs.head.Hash} {gitfs.head.Branch}");

            (var lmer,var lch, var lnew, var ldel)=gitfs.TrackWorkingCopy();
            gitfs.track.WriteTrack(); 

            bool done1 = PF("conflicting files:",lmer);
            bool done2 = PF("changed files:",lch);
            bool done3 = PF("new files:",lnew);
            bool done4 = PF("deleted files:",ldel);
            if (!(done1 || done2 || done3 || done4))
                Console.WriteLine("working tree is clean");
                
        }
    }
}