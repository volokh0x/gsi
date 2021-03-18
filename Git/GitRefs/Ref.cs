using System;
using System.IO;
using System.Text.RegularExpressions;

namespace gsi
{
    class Ref : IRef
    {
        public string RefPath;
        public string Hash;
        public string Name;
        public Ref(string path)
        {
            RefPath=path;
            Name=new Regex(".*(refs/.*)$").Match(path).Groups[1].Value;
        }
        public void SetRef(string hash)
        {
            Hash=hash;
            File.WriteAllText(RefPath, $"{hash}\n");
        }
        public Commit GetCommit()
        {
            return null;
        }
        public string GetCommitHash()
        {
            Hash=Regex.Replace(File.ReadAllText(RefPath), @"\s+", string.Empty);
            return Hash;
        }
    }
    // class Ref : GitPathMember, IRef
    // {
    //     public Object GetObject()
    //     {
    //         string object_path=gitp.ObjectFullPath(GetHash());
    //         (byte[] data, ObjectType objt)=Object.ReadObject(object_path);
    //         switch(objt)
    //         {
    //             case ObjectType.blob:
    //                 return new Blob(object_path, data);
    //             case ObjectType.tree:
    //                 return new Tree(object_path, data);
    //             case ObjectType.commit:
    //                 return new Commit(object_path, data);
    //             default:
    //                 return null;
    //         }
    //     }
}