using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Unix;

namespace gsi
{
    struct TreeEntry
    {
        public int mode;
        public string name;
        public string hash;
        public override string ToString()
        {
            string type = Tree.IsTreeMode(mode)?"🗀":"🗎";
            return $"{type} {hash} {name}";
        }
    }
    class Tree : Object
    {
        public static bool IsTreeMode(int mode)
        {
            return mode>>12==4; 
        }

        public List<TreeEntry> Entries;
        private byte[] HashedContent;

        public Tree(GitFS gitfs, string hash)
        {
            this.gitfs=gitfs;
            Hash=hash;
            ReadTree();
        }
        public Tree(GitFS gitfs, List<IndexEntry> ies, string root)
        {
            this.gitfs=gitfs;
            Entries=new List<TreeEntry>();

            while(ies.Count!=0)
            {
                var ie = ies.First();
                var relpath = Path.GetRelativePath(root,ie.path);
                
                TreeEntry te;
                var mas = relpath.Split(Path.DirectorySeparatorChar);
                var name=mas[0];
                if (mas.Length==1)
                {
                    // blob
                    te = new TreeEntry
                    {
                        mode=0b1000_000_100_100_100,
                        name=name,
                        hash=ie.hash
                    };
                    ies.Remove(ie);
                }
                else 
                {
                    // tree
                    var new_root=Path.Combine(root,name);
                    var x = Path.GetRelativePath(gitfs.gitp.Root,new_root);
                    var tree = new Tree(gitfs,ies.Where(ie=>ie.path.StartsWith(x)).ToList(),new_root);
                    var hash = tree.WriteTree();
                    te = new TreeEntry
                    {
                        mode=0b0100_000_000_000_000,
                        name=name,
                        hash=hash
                    }; 
                    ies.RemoveAll(ie=>ie.path.StartsWith(x));
                }
                Entries.Add(te);
            }
        }
        public void ReadTree()
        {
            (byte[] data, ObjectType objt) = Object.ReadObject(OPath);
            if (objt!=ObjectType.tree) throw new Exception("not a tree");
            
            int i=0, end; Entries=new List<TreeEntry>();
            while (true)
            {
                end=Array.IndexOf(data, (byte)0, i);
                if (end==-1)
                    break;
                byte[] header=new byte[end-i];
                Buffer.BlockCopy(data, i, header, 0, header.Length);
                string[] mas = Encoding.UTF8.GetString(header).Split(' ');
                var te = new TreeEntry();
                te.mode=Convert.ToInt32(mas[0], 8);
                te.name=mas[1];
                byte[] hash=new byte[20];
                Buffer.BlockCopy(data, end+1, hash, 0, hash.Length);
                te.hash=string.Concat(hash.Select(b => b.ToString("x2")));
                Entries.Add(te);
                i=end+1+20;
            }
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