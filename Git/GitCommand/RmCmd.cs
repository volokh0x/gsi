using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void RmCmd(List<string> ppatterns, bool f, bool r)
        {   
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();

            (var lch, var lnew, var ldel)=gitfs.index.GetStatus();

            foreach(var ppattern in ppatterns) 
            {
                var rm_fpaths=gitfs.index.GetMatchingEntries(gitfs.gitp.RelToRoot(Path.Combine(Environment.CurrentDirectory,ppattern))).Select(ie=>ie.path).ToList();
                
                if (f)
                    throw new Exception("not supported");
                if (rm_fpaths.Count==0)
                    throw new Exception($"{ppattern} did not match any files");
                if (Directory.Exists(ppattern) && !r)
                    throw new Exception($"not removing {ppattern} without -r");
                if (rm_fpaths.Intersect(lch).Count()!=0 || rm_fpaths.Intersect(lnew).Count()!=0)
                    throw new Exception($"these files have changes: {string.Join("\n",rm_fpaths)}");
                foreach(var fpath in rm_fpaths)
                {
                    
                    File.Delete(fpath);
                    gitfs.index.DelEntry(fpath);
                    Console.WriteLine($"{fpath} removed");
                }
                if (Directory.Exists(ppattern)) Directory.Delete(ppattern);
                gitfs.index.WriteIndex();
            }
           // Directory.SetCurrentDirectory(gitfs.gitp.Root);
  
        }
    }
}