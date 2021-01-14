using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace gsi
{
    partial class X
    {
        public static (ObjectType, byte[]) ReadObject(string sha1_prefix)  
        {
            string path = FindObject(sha1_prefix);
            byte[] full_data; int fd_len;

            (full_data, fd_len)=GitPath.Decompress(path);
            
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
