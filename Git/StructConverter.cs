using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace gsi 
{
    class StructConverter
    {
        // L - uint
        // s - string
        // H - ushort
        // IndexHeader: >4sLL
        // IndexEntry: >LLLLLLLLLL20sH
        private static void LtB(byte[] src, int start, byte[] dst, bool is_big_endian=true)
        {
            Buffer.BlockCopy(src, start, dst, 0, dst.Length);
            if (is_big_endian)
                Array.Reverse(dst);
        }
        private static byte[] Unpack(byte[] src, int start, string fmt, bool is_big_endian=true)
        {
            byte[] dst=null;
            if (fmt=="L")
            {
                dst=new byte[4];
                LtB(src, start, dst, is_big_endian);
            }
            else if (fmt=="H")
            {
                dst=new byte[2];
                LtB(src, start, dst, is_big_endian); 
            }
            return dst;
        }
        private static string UnpackStr(byte[] src, int start, int count)
        {
            return Encoding.UTF8.GetString(src, start, count);
        }
        private static string UnpackHash(byte[] src, int start)
        {
            return GitPath.HexSha1(src, start);
        }
        public static void Unpack(out IndexHeader ih, byte[] data, int start)
        {
            ih = new IndexHeader{signature=UnpackStr(data, start, 4), 
                                version=BitConverter.ToUInt32(Unpack(data, start+4, "L")), 
                                num_entries=BitConverter.ToUInt32(Unpack(data, start+8, "L"))};
        }
        public static void Unpack(out IndexEnry ie, byte[] data, int start)
        {
            ie = new IndexEnry{ctime_s=BitConverter.ToUInt32(Unpack(data, start+0, "L")),
                            ctime_n=BitConverter.ToUInt32(Unpack(data, start+4, "L")),
                            mtime_s=BitConverter.ToUInt32(Unpack(data, start+8, "L")),
                            mtime_n=BitConverter.ToUInt32(Unpack(data, start+12, "L")),
                            dev=BitConverter.ToUInt32(Unpack(data, start+16, "L")),
                            ino=BitConverter.ToUInt32(Unpack(data, start+20, "L")),
                            mode=BitConverter.ToUInt32(Unpack(data, start+24, "L")),
                            uid=BitConverter.ToUInt32(Unpack(data, start+28, "L")),
                            gid=BitConverter.ToUInt32(Unpack(data, start+32, "L")),
                            size=BitConverter.ToUInt32(Unpack(data, start+36, "L")),
                            sha1=UnpackHash(data, start+40),
                            flags=BitConverter.ToUInt16(Unpack(data, start+60, "H")),
                            path=UnpackStr(data, start+62, Array.IndexOf(data, (byte)0, start+62)-start-62)};
        }
        public static byte[] Pack(IndexHeader ih)
        {
            return null;
        }
    }
}