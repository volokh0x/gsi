using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class GitCommand 
    {
        public static void AddCmd(List<string> paths)
        {
            paths=paths.Select(path => Path.GetFullPath(path)).ToList();
            
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            // user files, if dir then recursive
            List<string> fpaths=new List<string>();
            foreach(var path in paths)
                if (File.Exists(path))
                    fpaths.Add(path);
                else if (Directory.Exists(path))
                    fpaths.AddRange(Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories));
                else 
                    throw new Exception($"{path} was not found");
            if (fpaths.Count==0)
            {
                Console.WriteLine("nothing specified, nothing added");
                return;
            }
            
            if (gitfs.index==null)
                gitfs.index=new Index(gitfs,false);
            foreach(var path in fpaths)
            {
                Blob blob=new Blob(gitfs,path,true);
                string hash = blob.WriteBlob();
                gitfs.index.AddEntry(gitfs.gitp.RelToRoot(path), hash);
                Console.WriteLine($"{path} added");
            } 
            gitfs.index.WriteIndex();       
        }
    }
}