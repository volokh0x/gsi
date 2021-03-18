using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    static partial class GitCommand 
    {
        public static void Add(List<string> paths)
        {
            paths=paths.Select(path => Path.GetFullPath(path)).ToList();
            GitFS gitfs=new GitFS();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            // valid non-bare repo
            gitfs.gitp.AssertValidRoot();
            gitfs.config.AssertNotBare();

            // multiple files, if dir then recursive
            // gitfs.gitp.index.AddUserFile(fpath)    
                  
        }
        private static void AddFiles(List<string> fpaths)
        {
            //     (byte[] data, string hash)=Object.HashObject(File.ReadAllBytes(path), ObjectType.blob);
            //     Object.WriteObject(data, ObjectType.blob, gitp.ObjectFullPath(hash));  
        }
    }
}