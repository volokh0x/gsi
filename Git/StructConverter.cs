using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace gsi 
{
    class StructConverter
    {
        // L - uint
        // s - string
        // H - ushort
        // IndexHeader: >4sLL
        // IndexEntry: >LLLLLLLLLL20sH
        public static void Unpack(out IndexHeader ih, byte[] bytes, int starti)
        {
            byte[] mversion = new byte[4], mnum_entries = new byte[4]; 

            // version
            Buffer.BlockCopy(bytes, starti+4, mversion, 0, mversion.Length);
            Array.Reverse(mversion);
            // num_entries
            Buffer.BlockCopy(bytes, starti+8, mnum_entries, 0, mnum_entries.Length);
            Array.Reverse(mnum_entries);

            ih = new IndexHeader{signature=Encoding.UTF8.GetString(bytes, starti, 4), 
                                version=BitConverter.ToUInt32(mversion), 
                                num_entries=BitConverter.ToUInt32(mnum_entries)};
        }
        public static void Unpack(out IndexEnry ie, byte[] bytes, int starti)
        {
            byte[] mctime_s = new byte[4],
                    mctime_n = new byte[4],
                    mmtime_s = new byte[4],
                    mmtime_n = new byte[4],
                    mdev= new byte[4],
                    mino = new byte[4],
                    mmode = new byte[4],
                    muid = new byte[4],
                    mgid = new byte[4],
                    msize = new byte[4],
                    mflags = new byte[2];
            // uint ctime_s;
            Buffer.BlockCopy(bytes, starti+0, mctime_s, 0, mctime_s.Length);
            Array.Reverse(mctime_s);
            // uint ctime_n;
            Buffer.BlockCopy(bytes, starti+4, mctime_n, 0, mctime_n.Length);
            Array.Reverse(mctime_n);
            // uint mtime_s;
            Buffer.BlockCopy(bytes, starti+8, mmtime_s, 0, mmtime_s.Length);
            Array.Reverse(mmtime_s);
            // uint mtime_n;
            Buffer.BlockCopy(bytes, starti+12, mmtime_n, 0, mmtime_n.Length);
            Array.Reverse(mmtime_n);
            // uint dev;
            Buffer.BlockCopy(bytes, starti+16, mdev, 0, mdev.Length);
            Array.Reverse(mdev);
            // uint ino;
            Buffer.BlockCopy(bytes, starti+20, mino, 0, mino.Length);
            Array.Reverse(mino);
            // uint mode;
            Buffer.BlockCopy(bytes, starti+24, mmode, 0, mmode.Length);
            Array.Reverse(mmode);
            // uint uid;
            Buffer.BlockCopy(bytes, starti+24, muid, 0, muid.Length);
            Array.Reverse(muid);
            // uint gid;
            Buffer.BlockCopy(bytes, starti+28, mgid, 0, mgid.Length);
            Array.Reverse(mgid);
            // uint size;
            Buffer.BlockCopy(bytes, starti+32, msize, 0, msize.Length);
            Array.Reverse(msize);
            // string sha1;
            // ...
            // ushort flags;
            Buffer.BlockCopy(bytes, starti+56, mflags, 0, mflags.Length);
            Array.Reverse(mflags);
            // string path;
            // ...
            ie = new IndexEnry{ctime_s=BitConverter.ToUInt32(mctime_s),
                            ctime_n=BitConverter.ToUInt32(mctime_n),
                            mtime_s=BitConverter.ToUInt32(mmtime_s),
                            mtime_n=BitConverter.ToUInt32(mmtime_n),
                            dev=BitConverter.ToUInt32(mdev),
                            ino=BitConverter.ToUInt32(mino),
                            mode=BitConverter.ToUInt32(mmode),
                            uid=BitConverter.ToUInt32(muid),
                            gid=BitConverter.ToUInt32(mgid),
                            size=BitConverter.ToUInt32(msize),
                            sha1=Encoding.UTF8.GetString(bytes, starti+36, 20),
                            flags=BitConverter.ToUInt16(mflags),
                            path=Encoding.UTF8.GetString(bytes, starti+58, Array.IndexOf(bytes, (byte)0, starti+58)-starti-58)};
        }
        public static byte[] Pack(IndexHeader ih)
        {
            return null;
        }
    }
}