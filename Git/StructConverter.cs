using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

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
            return X.Sha1tHex(src, start);
        }
        public static void UnpackIndexHeader(out IndexHeader ih, byte[] data, int start)
        {
            ih = new IndexHeader{signature=UnpackStr(data, start, 4), 
                                version=BitConverter.ToUInt32(Unpack(data, start+4, "L")), 
                                num_entries=BitConverter.ToUInt32(Unpack(data, start+8, "L"))};
        }
        public static void UnpackIndexEnry(out IndexEnry ie, byte[] data, int start)
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
        private static byte[] Pack(UInt16 x, bool is_big_endian=true)
        {
            byte[] mas = BitConverter.GetBytes(x);
            if (is_big_endian)
                Array.Reverse(mas);
            return mas;
        }
        private static byte[] Pack(UInt32 x, bool is_big_endian=true)
        {
            byte[] mas = BitConverter.GetBytes(x);
            if (is_big_endian)
                Array.Reverse(mas);
            return mas;
        }
        private static byte[] Pack(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
        public static byte[] PackTree(List<IndexEnry> mas_ie){
            List<byte> L = new List<byte>();
            foreach(var ie in mas_ie)
            {
                string mode = X.GetOctal6(ie.mode);
                L.AddRange(Pack(mode));
                L.AddRange(Pack(" "));
                L.AddRange(Pack(ie.path));
                L.Add((byte)0);
                L.AddRange(X.Sha1tBytes(ie.sha1));
            }
            return L.ToArray();
        }
        public static byte[] PackIndex(List<IndexEnry> mas_ie)
        {
            List<byte> res=new List<byte>();
            // header
            res.AddRange(Pack("DIRC"));
            res.AddRange(Pack(Convert.ToUInt32(2)));
            res.AddRange(Pack(Convert.ToUInt32(mas_ie.Count)));
            // body
            foreach(var ie in mas_ie)
            {
                res.AddRange(Pack(ie.ctime_s));
                res.AddRange(Pack(ie.ctime_n));
                res.AddRange(Pack(ie.mtime_s));
                res.AddRange(Pack(ie.mtime_n));
                res.AddRange(Pack(ie.dev));
                res.AddRange(Pack(ie.ino));
                res.AddRange(Pack(ie.mode));
                res.AddRange(Pack(ie.uid));
                res.AddRange(Pack(ie.gid));
                res.AddRange(Pack(ie.size));
                res.AddRange(X.Sha1tBytes(ie.sha1));
                res.AddRange(Pack(ie.flags));
                byte[] mpath=Pack(ie.path);
                res.AddRange(mpath); 
                int length=((62 + mpath.Length+ 8) / 8) * 8;
                for (int _=0; _<length-62-mpath.Length; _++)
                    res.Add((byte)0);
            }
            // tail sha1
            res.AddRange(SHA1.Create().ComputeHash(res.ToArray()));
            return res.ToArray();
        }
    }
}