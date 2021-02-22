using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace gsi
{
    partial class X
    {
        public static string WriteObject(byte[] data, ObjectType obj_type, bool write=true)
        {
            byte[] header = Encoding.UTF8.GetBytes($"{obj_type} {data.Length}");
            byte[] full_data = new byte[header.Length+1+data.Length];

            Buffer.BlockCopy(header, 0, full_data, 0, header.Length);  
            full_data[header.Length]=0;
            Buffer.BlockCopy(data, 0, full_data, header.Length+1, data.Length);

            string hash = Sha1tHex(SHA1.Create().ComputeHash(full_data));

            if (write)
            {
                string path = GitPath.ObjectFullPath(hash);
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));;   
                    Compress(path, full_data);   
                    File.SetAttributes(path, FileAttributes.ReadOnly);
                }
            }
            
            return hash;
        }
    }
}