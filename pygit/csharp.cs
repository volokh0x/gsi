using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace gsi
{
    class X
    {
        private static string repo {get => Directory.GetCurrentDirectory();}
        private static char _s = Path.DirectorySeparatorChar;
        public static void Init(string repo) 
        {
            Directory.CreateDirectory(Path.Combine(repo, ".git", "refs", "heads"));
            Directory.CreateDirectory(Path.Combine(repo, ".git", "refs", "tags"));
            Directory.CreateDirectory(Path.Combine(repo, ".git", "objects"));
            File.WriteAllText(Path.Combine(repo, ".git", "HEAD"), 
                $"ref: refs{_s}heads{_s}master");
            Console.WriteLine($"initialized empty repository: {repo}");
        }
        public static string HashObject(byte[] data, string obj_type, bool write=true)
        {
            byte[] header = Encoding.UTF8.GetBytes($"{obj_type} {data.Length}");
            byte[] full_data = new byte[header.Length+1+data.Length];

            Buffer.BlockCopy(header, 0, full_data, 0, header.Length);
            full_data[header.Length]=0;
            Buffer.BlockCopy(data, 0, full_data, header.Length+1, data.Length);

            SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(header);
            string hash_str = string.Concat(hash.Select(b => b.ToString("x2")));

            if (write)
            {
                string path = Path.Combine(repo, ".git", "objects", hash_str.Substring(0,2), hash_str.Substring(2));
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    using (FileStream dest_stream = new FileStream(path, FileMode.Create))
                        using (DeflateStream deflate_stream = new DeflateStream(dest_stream, CompressionMode.Compress))
                            deflate_stream.Write(full_data, 0, full_data.Length);
                }
            }
            return hash_str;
        }
    }
}
