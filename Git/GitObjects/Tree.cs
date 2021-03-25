using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Mono.Unix;

namespace gsi
{
    struct TreeEntry
    {
        public int mode;
        public string name;
        public string hash;
    }
    class Tree : Object
    {
        public static string WriteTreeGraph(GitFS gitfs)
        {
            return new Tree
            (
                gitfs, gitfs.index.Entries, gitfs.gitp.Root
            ).WriteTree();
        }
        public static bool IsTreeMode(int mode)
        {
            return (int)Mono.Unix.FileTypes.Directory==mode<<12; // ?? shift left or right
        }

        public List<TreeEntry> Entries;
        private byte[] HashedContent;

        public Tree(GitFS gitfs, List<IndexEntry> ies, string pardir)
        {
            this.gitfs=gitfs;
            Entries=new List<TreeEntry>();
            
            while (ies.Count>0)
            {
                var ief=ies.First();  
                
                string entry_path = Path.GetFullPath(ief.path);
                string path_from_par = Path.GetRelativePath(pardir,entry_path);

                if (path_from_par.Contains(Path.DirectorySeparatorChar))
                {
                    // tree
                    string name=path_from_par.Split(Path.DirectorySeparatorChar)[0];
                    pardir=Path.Combine(pardir,name);
                    string hash=new Tree
                    (
                        gitfs, ies.Where(ie=>ie.path.StartsWith(gitfs.gitp.RelToRoot(pardir))).ToList(), pardir
                    ).WriteTree();
                    var te = new TreeEntry
                    {
                        mode=0b0100_000_000_000_000,
                        name=name,
                        hash=hash
                    };
                    Entries.Add(te);
                }
                else 
                {
                    // blob entry
                    var te = new TreeEntry
                    {
                        mode=0b1000_000_100_100_100,
                        name=path_from_par,
                        hash=ief.hash
                    };
                    Entries.Add(te);
                }
                ies.RemoveAll(ie=>ie.path==ief.path); 
            }
        }
        public void ReadTree()
        {
            (byte[] data, ObjectType objt) = Object.ReadObject(OPath);
            if (objt!=ObjectType.tree) throw new Exception("not a tree");
            // data to Content !!!
        }
        public string HashTree()
        {
            List<byte> data=new List<byte>();
            foreach(var te in Entries)
            {
                string modeS = StructConverter.IntToOctal6(te.mode);
                data.AddRange(StructConverter.PackStr(modeS));
                data.AddRange(StructConverter.PackStr(" "));
                data.AddRange(StructConverter.PackStr(te.name));
                data.Add((byte)0);
                data.AddRange(StructConverter.PackHash(te.hash));
            }
            (byte[] hashed_data, string hash) = Object.HashObject(data.ToArray(),ObjectType.tree);
            HashedContent=hashed_data; Hash=hash;
            return hash;
        }
        public string WriteTree(bool to_hash=true)
        {
            if (to_hash || HashedContent==null)
                HashTree();
            Object.WriteObject(HashedContent, ObjectType.tree, OPath); 
            return Hash;
        }
    }
}