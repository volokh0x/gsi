using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace gsi
{
    partial class X
    {
        public static string HashObject(byte[] data, ObjectType obj_type, bool write=true)
        {
            byte[] header = Encoding.UTF8.GetBytes($"{obj_type} {data.Length}");
            byte[] full_data = new byte[header.Length+1+data.Length];

            Buffer.BlockCopy(header, 0, full_data, 0, header.Length);  
            full_data[header.Length]=0;
            Buffer.BlockCopy(data, 0, full_data, header.Length+1, data.Length);

            string hash = GitPath.HexSha1(SHA1.Create().ComputeHash(header));

            if (write)
            {
                string path = GitPath.PathObject(hash);
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));;   
                    GitPath.Compress(path, full_data);   
                }
            }
            
            return hash;
        }
    }
}