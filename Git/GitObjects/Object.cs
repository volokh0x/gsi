using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace gsi
{
    enum ObjectType
    {
        blob,
        tree,
        commit,
        tag,
        track
    }
    class Object
    {
        protected GitFS gitfs;
        public string Hash;
        public string OPath {get => gitfs.gitp.PathFromHash(Hash);}

        public static (byte[],ObjectType) ReadObject(string path)
        {
            (byte[] full_data, int fd_len)=DecompressFromFile(path);
            
            int i = Array.IndexOf(full_data, (byte)0);
            byte[] header = new byte[i];
            Buffer.BlockCopy(full_data, 0, header, 0, header.Length);

            string[] mas = Encoding.UTF8.GetString(header).Split(' ');
            ObjectType objt=(ObjectType)Enum.Parse(typeof(ObjectType), mas[0]);
            byte[] data = new byte[fd_len-i-1];
            if (Convert.ToInt32(mas[1])!=data.Length)
                throw new Exception();
            Buffer.BlockCopy(full_data, i+1, data, 0, data.Length);
            return (data,objt); 
        }
        public static (byte[],string) HashObject(byte[] data, ObjectType objt)
        {
            byte[] header = Encoding.UTF8.GetBytes($"{objt} {data.Length}");
            byte[] full_data = new byte[header.Length+1+data.Length];

            Buffer.BlockCopy(header, 0, full_data, 0, header.Length);  
            full_data[header.Length]=0;
            Buffer.BlockCopy(data, 0, full_data, header.Length+1, data.Length);
            
            return (full_data, StructConverter.HashBToS(SHA1.Create().ComputeHash(full_data)));
        }
        public static void WriteObject(byte[] data, ObjectType objt, string path)
        {
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));;   
                CompressToFile(path, data);   
                File.SetAttributes(path, FileAttributes.ReadOnly);
            }
        }
        public static (byte[], int) DecompressFromFile(string path) 
        {
            List<byte> L=new List<byte>();
            using (FileStream source_stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var inflater = new InflaterInputStream(source_stream)) 
                {
                    byte[] data =new byte[source_stream.Length];
                    while (inflater.Available==1)
                    {
                        int dlen = inflater.Read(data, 0, data.Length);
                        byte[] data2=new byte[dlen];
                        Buffer.BlockCopy(data,0,data2,0,dlen);
                        L.AddRange(data2);
                    } 
                }                 
            } 
            return (L.ToArray(), L.Count);
        }  
        public static void CompressToFile(string path, byte[] data)
        {
            using (FileStream dest_stream = new FileStream(path, FileMode.CreateNew))
            {
                using (var deflater = new DeflaterOutputStream(dest_stream))
                {
                    deflater.Write(data, 0, data.Length);
                }  
            }
        }
    }
}