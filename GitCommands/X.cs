using System;
using System.IO;
// using System.IO.Compression;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace gsi
{
    enum ObjectType
    {
        blob,
        tree,
        commit,
        tag
    }

    class GitPath
    {
        public static string wpath {get => WorkPath();}
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
            // using (FileStream dest_stream = new FileStream(file, FileMode.Create)) 
            //     using (DeflateStream deflate_stream = new DeflateStream(dest_stream, CompressionMode.Compress))
            //         deflate_stream.Write(data, 0, data.Length); 
            using (FileStream dest_stream = new FileStream(file, FileMode.Create)) {
                using (var deflater = new DeflaterOutputStream(dest_stream))
                {
                    deflater.Write(data, 0, data.Length);
                }
            }
        }
        public static (byte[], int) Decompress(string file)
        {
            // byte[] data; int dlen;
            // using (FileStream source_stream = new FileStream(file, FileMode.Open))
            // {
            //     data =new byte[source_stream.Length];
            //     using (DeflateStream deflate_stream = new DeflateStream(source_stream, CompressionMode.Decompress))
            //         dlen = deflate_stream.Read(data, 0, data.Length);                   
            // }  
            // return (data, dlen);
            byte[] data; int dlen;
            using (FileStream source_stream = new FileStream(file, FileMode.Open))
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
    
    partial class X
    {
        // I exist )
    }
}