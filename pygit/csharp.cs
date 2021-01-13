using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace gsi
{
    enum ObjectType{
        blob,
        tree,
        commit,
        tag
    }
    
    class X
    {
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
        public static string HashObject(byte[] data, ObjectType obj_type, bool write=true)
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
                string path = Path.Combine(".git", "objects", hash_str.Substring(0,2), hash_str.Substring(2));
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
        public static string FindObject(string sha1_prefix)
        {
            if (sha1_prefix.Length < 2) 
                throw new ArgumentException();
            string obj_dir = Path.Combine(".git", "objects", sha1_prefix.Substring(0,2));
            string rest = sha1_prefix.Substring(2);
            string[] objects = Directory.GetFiles(obj_dir, $"{rest}*");
            if (objects.Length>2) 
                throw new Exception();
            return objects[0];
        }
        public static (ObjectType, byte[]) ReadObject(string sha1_prefix)  
        {
            string path = FindObject(sha1_prefix);
            byte[] full_data; int fd_len;
            using (FileStream source_stream = new FileStream(path, FileMode.Open))
            {
                full_data=new byte[source_stream.Length];
                using (DeflateStream deflate_stream = new DeflateStream(source_stream, CompressionMode.Decompress))
                    fd_len = deflate_stream.Read(full_data, 0, full_data.Length);                   
            }   
            int i = Array.IndexOf(full_data, (byte)0);
            byte[] header = new byte[i];
            Buffer.BlockCopy(full_data, 0, header, 0, i);

            string[] mas = Encoding.UTF8.GetString(header).Split(' ');
            ObjectType obj_type=(ObjectType)Enum.Parse(typeof(ObjectType), mas[0]); int size = Convert.ToInt32(mas[1]);
            byte[] data = new byte[fd_len-i-1];
            Buffer.BlockCopy(full_data, i+1, data, 0, data.Length);
            return (obj_type, data); 
        }
        
    }
}
