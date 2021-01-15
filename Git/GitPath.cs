using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace gsi
{
    
    class GitPath
    {
        public static string wpath {get => WorkPath();}
        
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
        public static string PathObject(string hash)
        {
            return Path.Combine(wpath, ".git", "objects", hash.Substring(0,2), hash.Substring(2));
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
        public static string HexSha1(byte[] hash)
        {
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }
}