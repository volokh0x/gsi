using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Mono.Unix;

namespace gsi
{
    struct IndexHeader
    {
        public string signature;
        public int version;
        public int num_entries;
    }
    struct IndexEntry
    {
        public int ctime_s;
        public int ctime_n;
        public int mtime_s;
        public int mtime_n;
        public int dev;
        public int ino;
        public int mode;
        public int uid;
        public int gid;
        public int size;
        public string hash;
        public short flags;
        public string path;
    }
    enum Stage
    {
        NONCONFL,
        BASE,
        RECEIVER,
        GIVER
    }
    class Index 
    {
        public static Stage StageFromFlags(short flags)
        {
            return (Stage)((flags>>12)&0b11);
        }
        
        public string IndexPath {get;}
        public IndexHeader indh;
        public List<IndexEntry> Entries = new List<IndexEntry>();
        public Index(string path, bool read_index=true)
        {
            IndexPath=path;
            if (read_index) ReadIndex();
        }
        public void ReadIndex()  
        {
            byte[] data;
            try
            {
                data=File.ReadAllBytes(IndexPath);
            } 
            catch(Exception) {return;}

            // check data consistency with hashes
            byte[] data_hash=new byte[20]; Buffer.BlockCopy(data, data.Length-20, data_hash, 0, data_hash.Length);
            byte[] tobe_hashed=new byte[data.Length-20]; Buffer.BlockCopy(data, 0, tobe_hashed, 0, tobe_hashed.Length);
            if (StructConverter.UnPackHash(data_hash)!=StructConverter.UnPackHash(SHA1.Create().ComputeHash(tobe_hashed)))
                throw new Exception();
            
            // header part
            indh = new IndexHeader{
                signature=StructConverter.UnPackStr(data, 0, 4), 
                version=StructConverter.UnPackInt32(data, 4), 
                num_entries=StructConverter.UnPackInt32(data, 8)
            };
            
            // header checks
            if (indh.signature!="DIRC")
                throw new Exception();
            if (indh.version!=2)
                throw new Exception();

            // entries part
            byte[] entry_data=new byte[data.Length-32]; Buffer.BlockCopy(data, 12, entry_data, 0, entry_data.Length);          
            int i=0;
            for (int _=0; _<indh.num_entries; _++)
            {
                var ie = new IndexEntry
                {
                    ctime_s=StructConverter.UnPackInt32(entry_data, i+0),
                    ctime_n=StructConverter.UnPackInt32(entry_data, i+4),
                    mtime_s=StructConverter.UnPackInt32(entry_data, i+8),
                    mtime_n=StructConverter.UnPackInt32(entry_data, i+12),
                    dev=StructConverter.UnPackInt32(entry_data, i+16),
                    ino=StructConverter.UnPackInt32(entry_data, i+20),
                    mode=StructConverter.UnPackInt32(entry_data, i+24),
                    uid=StructConverter.UnPackInt32(entry_data, i+28),
                    gid=StructConverter.UnPackInt32(entry_data, i+32),
                    size=StructConverter.UnPackInt32(entry_data, i+36),
                    hash=StructConverter.UnPackHash(entry_data, i+40),
                    flags=StructConverter.UnPackInt16(entry_data, i+60),
                    path=StructConverter.UnPackStr(entry_data, i+62, Array.IndexOf(entry_data, (byte)0, i+62)-i-62)
                };
                Entries.Add(ie);
                i+=((62 + ie.path.Length + 8) / 8) * 8;
            }
            if (Entries.Count!=indh.num_entries)
                throw new Exception($"{Entries.Count} (counted)!= {indh.num_entries} (in index)");
        }
        public void WriteIndex()
        {
            List<byte> res=new List<byte>();
            Entries = Entries.OrderBy(ie => ie.path).ToList();

            // header part
            res.AddRange(StructConverter.PackStr("DIRC"));
            res.AddRange(StructConverter.PackInt32(2));
            res.AddRange(StructConverter.PackInt32(Entries.Count));
            
            // entries part
            foreach(var ie in Entries)
            {
                res.AddRange(StructConverter.PackInt32(ie.ctime_s));
                res.AddRange(StructConverter.PackInt32(ie.ctime_n));
                res.AddRange(StructConverter.PackInt32(ie.mtime_s));
                res.AddRange(StructConverter.PackInt32(ie.mtime_n));
                res.AddRange(StructConverter.PackInt32(ie.dev));
                res.AddRange(StructConverter.PackInt32(ie.ino));
                res.AddRange(StructConverter.PackInt32(ie.mode));
                res.AddRange(StructConverter.PackInt32(ie.uid));
                res.AddRange(StructConverter.PackInt32(ie.gid));
                res.AddRange(StructConverter.PackInt32(ie.size));
                res.AddRange(StructConverter.PackHash(ie.hash));
                res.AddRange(StructConverter.PackInt16(ie.flags));
                byte[] bpath=StructConverter.PackStr(ie.path); int plen=bpath.Length; res.AddRange(bpath); 
                int length=((62 + plen+ 8) / 8) * 8;
                for (int _=0; _<length-62-plen; _++)
                    res.Add((byte)0);
            }
            
            // tail part
            res.AddRange(SHA1.Create().ComputeHash(res.ToArray()));
            
            // write to a file
            File.WriteAllBytes(IndexPath, res.ToArray());
        }
        public void AddEntry(string path, string hash)
        {
            Entries.RemoveAll(ie=>ie.path==path);
            
            short len=(short)Encoding.UTF8.GetBytes(path).Length;
            short flags=(short)(len&0b0000_111111111111);

            UnixFileInfo unixFileInfo = new UnixFileInfo(path);
            int tt = StructConverter.TimeStamp(new FileInfo(path).LastWriteTimeUtc);
            int mode = ((int)unixFileInfo.Protection)|((int)unixFileInfo.FileAccessPermissions);

            var ie = new IndexEntry
            {
                ctime_n=tt,
                ctime_s=0,
                mtime_n=tt,
                mtime_s=0,
                dev=(int)unixFileInfo.DeviceType,
                ino=(int)unixFileInfo.Inode,
                mode=mode,
                uid=(int)unixFileInfo.OwnerUserId,
                gid=(int)unixFileInfo.OwnerGroupId,
                size=(int)unixFileInfo.Length,
                hash=hash,
                flags=flags,
                path=path
            };
            Entries.Add(ie);
            Entries = Entries.OrderBy(ie => ie.path).ToList();
        }
        public List<IndexEntry> GetConfilctingEntries()
        {
            List<IndexEntry> L = new List<IndexEntry>();
            foreach(var ie in Entries)
                if (Index.StageFromFlags(ie.flags)==Stage.RECEIVER)
                    L.Add(ie);
            return L;
        }
    }
}