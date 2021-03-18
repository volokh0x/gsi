using System;
using System.Text;
using System.Linq;

namespace gsi 
{
    static class StructConverter
    {
        public static void CopyBytes(byte[] src, int start, byte[] dst, bool is_big_endian=true)
        {
            Buffer.BlockCopy(src, start, dst, 0, dst.Length);
            if (is_big_endian)
                Array.Reverse(dst);
        }
        
        public static short UnPackInt16(byte[] src, int start, bool is_big_endian=true)
        {
            byte[] dst=new byte[2];
            CopyBytes(src, start, dst, is_big_endian); 
            return Convert.ToInt16(dst);
        }
        public static int UnPackInt32(byte[] src, int start, bool is_big_endian=true)
        {
            byte[] dst=new byte[4];
            CopyBytes(src, start, dst, is_big_endian); 
            return Convert.ToInt32(dst);
        }
        public static string UnPackStr(byte[] src, int start, int count)
        {
            return Encoding.UTF8.GetString(src, start, count);
        }
        public static string UnPackHash(byte[] src, int start=0)
        {
            byte[] dst = new byte[20]; Buffer.BlockCopy(src, start, dst, 0, dst.Length);
            return string.Concat(dst.Select(b => b.ToString("x2")));
        }
        
        public static byte[] PackInt16(short x, bool is_big_endian=true)
        {
            byte[] dst = BitConverter.GetBytes(x);
            if (is_big_endian)
                Array.Reverse(dst);
            return dst;
        }
        public static byte[] PackInt32(int x, bool is_big_endian=true)
        {
            byte[] dst = BitConverter.GetBytes(x);
            if (is_big_endian)
                Array.Reverse(dst);
            return dst;
        }
        public static byte[] PackStr(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
        public static byte[] PackHash(string hash)
        {
            return Enumerable.Range(0, hash.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hash.Substring(x, 2), 16))
                            .ToArray();
        }
        
        public static string HashBToS(byte[] src, int start=0)
        {
            return UnPackHash(src,start);
        }
        public static byte[] HashSToB(string hash)
        {
            return PackHash(hash);
        }
        public static string IntToOctal6(int x)
        {
            string fmt = "000000";
            string oct=Convert.ToString(x,8); 
            return Convert.ToInt32(oct).ToString(fmt);
        }
        public static int TimeStamp(DateTime dt)
        {
            var diff = dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt32(Math.Floor(diff.TotalSeconds));
        }
    }
}