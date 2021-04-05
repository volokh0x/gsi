using System;
using System.IO;
using System.Text.RegularExpressions;

namespace gsi
{
    class Ref
    {
        private GitFS gitfs;
        public string Name; 
        public string RPath {get => gitfs.gitp.PathFromDir("refs", Name);}
        public string Hash;

        public Ref(GitFS gitfs, string Name, bool read_ref=true)
        {
            this.gitfs=gitfs;
            this.Name=Name;
            if (read_ref) ReadRef();
        }
        public string ReadRef()
        {
            Hash=Regex.Replace(File.ReadAllText(RPath), @"\s+", string.Empty);
            return Hash;
        }
        public void SetRef(string hash)
        {
            Hash=hash;
            File.WriteAllText(RPath, $"{Hash}\n");
        }
    }
}