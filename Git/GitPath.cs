using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace gsi
{
    class GitPath
    {
        public static readonly string cwd = Environment.CurrentDirectory;
        public static string root;
        public static Dictionary<string,string> DirRelPath;
        public static Dictionary<string,string> DirFullPath;
        public static string HEAD;
        public static string index ;
        public static string heads_master;
        static GitPath()
        {   
            DirRelPath = new Dictionary<string, string>();
            DirRelPath[".git"]=".git";
                DirRelPath["objects"]=Path.Combine(DirRelPath[".git"], "objects");
                DirRelPath["refs"]=Path.Combine(DirRelPath[".git"], "refs");
                    DirRelPath["heads"]=Path.Combine(DirRelPath["refs"], "heads");
                    DirRelPath["tags"]=Path.Combine(DirRelPath["refs"], "tags");
            GitPathInitRoot(null);
        }
        public static void GitPathInitRoot(string rpath=null)
        {
            root = (rpath==null? WorkPath():Path.GetFullPath(rpath));
            DirFullPath = new Dictionary<string, string>();
            foreach(KeyValuePair<string, string> el in DirRelPath)
                DirFullPath[el.Key]=Path.Combine(root, el.Value);
            HEAD = Path.Combine(DirFullPath[".git"], "HEAD");
            index = Path.Combine(DirFullPath[".git"], "index");
            heads_master = Path.Combine(DirFullPath["heads"], "master");
        }
        private static string WorkPath()
        {
            string dir=GitPath.cwd;
            while (true)
            {
                if (Directory.Exists(Path.Combine(dir, ".git")))
                    return dir;
                var info = Directory.GetParent(dir);
                if (info==null)
                    return GitPath.cwd;
                dir=info.FullName;
            }
        }
        public static string ObjectFullPath(string prefix)
        {
            return Path.Combine(DirFullPath["objects"], prefix.Substring(0,2), prefix.Substring(2));
        }
        public static string? GetLocalMasterHash()
        {
            try
            {
                return File.ReadAllText(GitPath.heads_master).Trim();
            }
            catch(Exception) {return null;}
        }
        public static bool IsDir(uint mode)
        {
            return mode<<12==(uint)Mono.Unix.FileTypes.Directory;
        }
        public static int FlagstStage(UInt16 flags)
        {
            return (flags>>12)&3; 
        }
        public static List<TreeEntry> ReadTree(string sha1_prefix=null, byte[] _data=null)
        {
            ObjectType objt; byte[] data=_data;
            if (sha1_prefix!=null)
            {
                (objt, data)=X.ReadObject(sha1_prefix);
                if (objt!=ObjectType.tree)
                    throw new Exception();
            }
            else if (_data==null)
                throw new Exception();
            int i=0, end; List<TreeEntry> L=new List<TreeEntry>();
            for (int _=0; _<1000; _++)
            {
                end=Array.IndexOf(data, (byte)0, i);
                if (end==-1)
                    break;
                byte[] header=new byte[end-i];
                Buffer.BlockCopy(data, i, header, 0, header.Length);
                string[] mas = Encoding.UTF8.GetString(header).Split(' ');
                var te = new TreeEntry();
                te.mode=Convert.ToUInt32(mas[0], 8);
                te.path=mas[1];
                byte[] hash=new byte[20];
                Buffer.BlockCopy(data, end+1, hash, 0, hash.Length);
                te.sha1=string.Concat(hash.Select(b => b.ToString("x2")));
                L.Add(te);
                i=end+1+20;
            }
            return L;
        }
        public static string WriteTree()
        {
            // !!! only top-level directory allowed !!!
            return X.HashObject(StructConverter.PackTree(ReadIndex()), ObjectType.tree);
        }
        public static List<IndexEnry> ReadIndex()
        {
            List<IndexEnry> L = new List<IndexEnry>();
            byte[] data;
            try
            {
                data=File.ReadAllBytes(index);
            } 
            catch(Exception) {return L;}

            byte[] data_hash=new byte[20]; Buffer.BlockCopy(data, data.Length-20, data_hash, 0, data_hash.Length);
            byte[] tobe_hashed=new byte[data.Length-20]; Buffer.BlockCopy(data, 0, tobe_hashed, 0, tobe_hashed.Length);
            if ( !Enumerable.SequenceEqual(data_hash, SHA1.Create().ComputeHash(tobe_hashed)) )
                throw new Exception();

            StructConverter.UnpackIndexHeader(out IndexHeader ih, data, 0);
            if ( !Enumerable.SequenceEqual(ih.signature, "DIRC") )
                throw new Exception();
            if (ih.version!=(uint)2)
                throw new Exception();
            uint num_entries=ih.num_entries;

            byte[] entry_data=new byte[data.Length-32]; Buffer.BlockCopy(data, 12, entry_data, 0, entry_data.Length);          
            int i=0;
            while (i+62<entry_data.Length)
            {
                StructConverter.UnpackIndexEnry(out IndexEnry ie, entry_data, i);
                L.Add(ie);
                i+=((62 + ie.path.Length + 8) / 8) * 8;
            }
            if ((ulong)L.Count!=num_entries)
                throw new Exception($"{(ulong)L.Count} (counted)!= {num_entries} (in index)");
            return L;
        }
        public static void WriteIndex(List<IndexEnry> mas_ie)
        {
            File.WriteAllBytes(index, StructConverter.PackIndex(mas_ie));
        }
        private static List<string> DirSearch(string relpath="")
        {
            List<string> L = new List<string>();
            string fullpath=Path.Combine(root, relpath);
            if (Path.GetFileName(fullpath)==".git") 
                return L;
            foreach (string f in Directory.GetFiles(fullpath))
                L.Add(Path.Combine(relpath,Path.GetFileName(f)));  
            foreach (string d in Directory.GetDirectories(fullpath))
            {
                L.AddRange(DirSearch(Path.GetFileName(d)));
            }  
            return L;
        }
        public static (List<string>, List<string>, List<string>) GetStatus()
        {
            List<string> paths=DirSearch();
            var entries_by_path = ReadIndex().ToDictionary(X=>X.path);
            List<string> entry_paths = new List<string>(entries_by_path.Keys);
            List<string> lch=new List<string>();
            foreach(var file in paths.Intersect(entry_paths))
            {
                if (X.HashObject(File.ReadAllBytes(Path.Combine(root,file)), ObjectType.blob, false)!=entries_by_path[file].sha1)
                    lch.Add(file);
            }
            List<string> lnew = new List<string>(paths.Except(entry_paths));
            List<string> ldel = new List<string>(entry_paths.Except(paths));
            lch.Sort(); lnew.Sort(); ldel.Sort();
            return (lch, lnew, ldel);
        } 
    }
}