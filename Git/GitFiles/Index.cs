using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        
        private GitFS gitfs;
        public string IndexPath {get => gitfs.gitp.PathFromRoot("index");}
        public IndexHeader indh;
        public List<IndexEntry> Entries = new List<IndexEntry>();
        public Index(GitFS gitfs, bool read_index=true)
        {
            this.gitfs=gitfs;
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
        public void DelEntry(string path)
        {
            Entries.RemoveAll(ie=>ie.path==path);
        }
        public List<IndexEntry> GetConfilctingEntries()
        {
            return Entries.Where(ie=>Index.StageFromFlags(ie.flags)==Stage.RECEIVER).ToList();
        }
        public List<IndexEntry> GetMatchingEntries(string ppattern)
        {
            return Entries.Where(ie=>Regex.IsMatch(ie.path,$"^{ppattern}")).ToList();
        }
        public (List<string>, List<string>, List<string>) GetStatus()
        {
            var dirInfo = new DirectoryInfo(gitfs.gitp.Root);
            var hiddenFolders = dirInfo.GetDirectories("*", SearchOption.AllDirectories)
                .Where(d => (d.Attributes & FileAttributes.Hidden) != 0)
                .Select(d => d.FullName);

            var fs_paths = dirInfo.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(f => (f.Attributes & FileAttributes.Hidden) == 0 && 
                    !hiddenFolders.Any(d => f.FullName.StartsWith(d))).Select(x => x.FullName)
                   .ToList();
            fs_paths=fs_paths.Select(path=>gitfs.gitp.RelToRoot(path)).ToList();

            var path_to_ie = Entries.ToDictionary(ie=>ie.path);
            List<string> ie_paths = new List<string>(Entries.Select(ie => ie.path).ToList());
            List<string> lch=new List<string>();
            foreach(var fpath in fs_paths.Intersect(ie_paths))
            {
                string full_path=Path.Combine(gitfs.gitp.Root,fpath);
                (byte[] _, string fs_hash)=Object.HashObject(File.ReadAllBytes(full_path), ObjectType.blob);
                if (fs_hash!=path_to_ie[fpath].hash)
                    lch.Add(fpath);
            }
            List<string> lnew = new List<string>(fs_paths.Except(ie_paths));
            List<string> ldel = new List<string>(ie_paths.Except(fs_paths));
            lch.Sort(); lnew.Sort(); ldel.Sort();
            return (lch, lnew, ldel);
        } 
    }
}