using System;
using System.IO;

namespace gsi
{
    class Head : IRef
    {
        string HeadPath;
        string Hash;
        string Content;
        public Head(string path)
        {
            HeadPath=path;
        } 
        public Head(string path, string hash)
        {
            HeadPath=path;
            SetHead(hash);
        }
        public Head(string path, Ref iref)
        {
            HeadPath=path;
            SetHead(iref);
        }
        public void SetHead(string hash)
        {
            Hash=hash;
            Content=$"{hash}\n";
        }
        public void SetHead(Ref iref)
        {
            Content=$"ref: {iref.Name}\n";
        }
        public void WriteHead()
        {
            File.WriteAllText(HeadPath,Content);
        }
        public Commit GetCommit()
        {
            return null;
        }
        public string GetCommitHash()
        {
            // read from file !!!
            // Hash=Regex.Replace(File.ReadAllText(RefPath), @"\s+", string.Empty);
            // return Hash;
            return null;
        }
    }
}