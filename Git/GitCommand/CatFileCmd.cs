using System;
using System.IO;

namespace gsi
{
    static partial class GitCommand 
    {
        public static void CatFileCmd(string hash_prefix) 
        {
            // valid non-bare repo
            GitFS gitfs=new GitFS(Environment.CurrentDirectory);
            gitfs.gitp.AssertValidRoot();
            if (gitfs.config_set.config_pr!=null) gitfs.config_set.config_pr.AssertNotBare();
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            string obj_path = gitfs.gitp.PathFromHash(hash_prefix);
            string hash=gitfs.gitp.HashFromPath(obj_path);
            if (obj_path==null)
            {
                Console.WriteLine($"no objects found for {hash_prefix}");
                return;
            }
            (byte[] data, ObjectType objectType)=Object.ReadObject(obj_path);
            switch(objectType)
            {
                case ObjectType.blob:
                    var blob = new Blob(gitfs,obj_path,false);
                    Console.Write(blob.Content);
                    break;    
                case ObjectType.tree:
                    var tree = new Tree(gitfs,hash);
                    foreach(var te in tree.Entries)
                        Console.WriteLine(te);
                    break;        
                case ObjectType.commit:
                    var commit = new Commit(gitfs,hash);
                    Console.Write(commit);
                    break;
            }
        }
    }
}