using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace gsi
{
    partial class X
    {
        public static (ObjectType, byte[]) ReadObject(string prefix)  
        {
            string path = FindObject(prefix);
            byte[] full_data; int fd_len;

            (full_data, fd_len)=GitPath.Decompress(path);
            
            int i = Array.IndexOf(full_data, (byte)0);
            byte[] header = new byte[i];
            Buffer.BlockCopy(full_data, 0, header, 0, header.Length);

            string[] mas = Encoding.UTF8.GetString(header).Split(' ');
            ObjectType obj_type=(ObjectType)Enum.Parse(typeof(ObjectType), mas[0]);
            byte[] data = new byte[fd_len-i-1];
            if (Convert.ToInt32(mas[1])!=data.Length)
                throw new Exception();
            Buffer.BlockCopy(full_data, i+1, data, 0, data.Length);
            return (obj_type, data); 
        }
    }
}
