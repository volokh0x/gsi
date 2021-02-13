using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace gsi
{
    partial class X
    { 
        public static string GetOctal6(uint x)
        {
            string fmt = "000000";
            string oct=Convert.ToString(x,8); 
            return Convert.ToInt32(oct).ToString(fmt);
        }
        public static uint GetTimeStamp(DateTime dt)
        {
            var diff = dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToUInt32(Math.Floor(diff.TotalSeconds));
        }
        public static string Sha1tHex(byte[] hash, int start)
        {
            byte[] hash2=new byte[20]; Buffer.BlockCopy(hash, start, hash2, 0, hash2.Length);
            return string.Concat(hash2.Select(b => b.ToString("x2")));
        }
        public static string Sha1tHex(byte[] hash)
        {
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
        public static byte[] Sha1tBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
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
            List<byte> L=new List<byte>();
            using (FileStream source_stream = new FileStream(file, FileMode.Open, FileAccess.Read))
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
    }
}
