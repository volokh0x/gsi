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
        // header = '{} {}'.format(obj_type, len(data)).encode()
        // full_data = header + b'\x00' + data
        // sha1 = hashlib.sha1(full_data).hexdigest()
        // if write:
        //     path = os.path.join('.git', 'objects', sha1[:2], sha1[2:])
        //     if not os.path.exists(path):
        //         os.makedirs(os.path.dirname(path), exist_ok=True)
        //         write_file(path, zlib.compress(full_data))
        // return sha1
        public static string HashObject(byte[] data, ObjectType obj_type, bool write=true)
        {
            byte[] header = Encoding.UTF8.GetBytes($"{obj_type} {data.Length}");
            byte[] full_data = new byte[header.Length+1+data.Length];

            Buffer.BlockCopy(header, 0, full_data, 0, header.Length);  
            full_data[header.Length]=0;
            Buffer.BlockCopy(data, 0, full_data, header.Length+1, data.Length);

            string hash = GitPath.HexSha1(SHA1.Create().ComputeHash(full_data));

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