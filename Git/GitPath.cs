using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace gsi
{
    class GitPath
    {
        public static string wpath {get => WorkPath();}
        public static string index {get => Path.Combine(wpath, ".git", "index");}
        private static string WorkPath()
        {
            string cd=Environment.CurrentDirectory;
            while (true)
            {
                if (Directory.Exists(Path.Combine(cd, ".git")))
                    return cd;
                var info = Directory.GetParent(cd);
                if (info==null)
                    throw new Exception();
                cd=info.FullName;
            }
        }
        public static string HexSha1(byte[] hash, int start)
        {
            byte[] hash2=new byte[20]; Buffer.BlockCopy(hash, start, hash2, 0, hash2.Length);
            return string.Concat(hash2.Select(b => b.ToString("x2")));
        }
        public static string HexSha1(byte[] hash)
        {
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
        public static byte[] ByteSha1(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
        }
        public static string PathObject(string hash)
        {
            return Path.Combine(wpath, ".git", "objects", hash.Substring(0,2), hash.Substring(2));
        }
        public static bool IsDir(int mode)
        {
            return (mode>>12)==4;
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
                te.mode=Convert.ToInt32(mas[0], 8);
                te.path=mas[1];
                byte[] hash=new byte[20];
                Buffer.BlockCopy(data, end+1, hash, 0, hash.Length);
                te.sha1=string.Concat(hash.Select(b => b.ToString("x2")));
                L.Add(te);
                i=end+1+20;
            }
            return L;
        }
        public static List<IndexEnry> ReadIndex()
        {
            byte[] data;
            try
            {
                data=File.ReadAllBytes(index);
            } 
            catch(Exception) {return null;}

            byte[] data_hash=new byte[20]; Buffer.BlockCopy(data, data.Length-20, data_hash, 0, data_hash.Length);
            byte[] tobe_hashed=new byte[data.Length-20]; Buffer.BlockCopy(data, 0, tobe_hashed, 0, tobe_hashed.Length);
            if ( !Enumerable.SequenceEqual(data_hash, SHA1.Create().ComputeHash(tobe_hashed)) )
                throw new Exception();

            StructConverter.Unpack(out IndexHeader ih, data, 0);
            if ( !Enumerable.SequenceEqual(ih.signature, "DIRC") )
                throw new Exception();
            if (ih.version!=(uint)2)
                throw new Exception();
            uint num_entries=ih.num_entries;

            List<IndexEnry> L = new List<IndexEnry>();
            byte[] entry_data=new byte[data.Length-32]; Buffer.BlockCopy(data, 12, entry_data, 0, entry_data.Length);          
            int i=0;
            while (i+62<entry_data.Length)
            {
                StructConverter.Unpack(out IndexEnry ie, entry_data, i);
                L.Add(ie);
                i+=((62 + ie.path.Length + 8) / 8) * 8;
            }
            if ((ulong)L.Count!=num_entries)
                throw new Exception($"{(ulong)L.Count} (counted)!= {num_entries} (in index)");
            return L;
        }
        public static void WriteIndex(List<IndexEnry> mas_ie)
        {
            File.WriteAllBytes(index, StructConverter.Pack(mas_ie));
        }
        private static List<string> DirSearch(string relpath="")
        {
            List<string> L = new List<string>();
            string fullpath=Path.Combine(wpath, relpath);
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
                if (X.HashObject(File.ReadAllBytes(Path.Combine(wpath,file)), ObjectType.blob, false)!=entries_by_path[file].sha1)
                    lch.Add(file);
            }
            List<string> lnew = new List<string>(paths.Except(entry_paths));
            List<string> ldel = new List<string>(entry_paths.Except(paths));
            lch.Sort(); lnew.Sort(); ldel.Sort();
            return (lch, lnew, ldel);
        }
        public static void Compress(string file, byte[] data)
        {
            using (FileStream dest_stream = new FileStream(file, FileMode.CreateNew))
            {
                using (var deflater = new DeflaterOutputStream(dest_stream))
                {
                    deflater.Write(data, 0, data.Length);
                }  
            }
            File.SetAttributes(file, FileAttributes.ReadOnly);
        }
        public static (byte[], int) Decompress(string file)
        {
            byte[] data; int dlen;
            using (FileStream source_stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (var inflater = new InflaterInputStream(source_stream)) 
                {
                    data =new byte[source_stream.Length];
                    dlen = inflater.Read(data, 0, data.Length);
                }                 
            }  
            return (data, dlen);
        }   
    }
}