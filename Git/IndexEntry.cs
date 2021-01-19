namespace gsi
{
    struct IndexHeader
    {
        public string signature;
        public uint version;
        public uint num_entries;
    }
    struct IndexEnry
    {
        public uint ctime_s;
        public uint ctime_n;
        public uint mtime_s;
        public uint mtime_n;
        public uint dev;
        public uint ino;
        public uint mode;
        public uint uid;
        public uint gid;
        public uint size;
        public string sha1;
        public ushort flags;
        public string path;
    }
}