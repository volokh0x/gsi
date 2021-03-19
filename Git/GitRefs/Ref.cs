using System;
using System.IO;
using System.Text.RegularExpressions;

namespace gsi
{
    class Ref
    {
        public string RefPath;
        public string Hash;
        public string Name;
        public Ref(string path, bool read_ref=false)
        {
            RefPath=path;
            Name=new Regex(".*refs/(.*)$").Match(path).Groups[1].Value;
            if (read_ref) ReadRef();
        }
        public string ReadRef()
        {
            Hash=Regex.Replace(File.ReadAllText(RefPath), @"\s+", string.Empty);
            return Hash;
        }
        public void WriteRef(string hash)
        {
            Hash=hash;
            File.WriteAllText(RefPath, $"{hash}\n");
        }
    }
}