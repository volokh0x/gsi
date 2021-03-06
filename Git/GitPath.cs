using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace gsi
{
    class GitPath
    {
        public string Root;
        public bool IsBare;
        public Dictionary<string,string> DirPath = new Dictionary<string, string>();
        public GitPath(string root, bool is_bare=false)
        {   
            Root=root;
            IsBare=is_bare;
            if (root==null) return;

            string git_folder=IsBare?"":".git";
            DirPath["objects"]=Path.Combine(root, git_folder, "objects");
            Directory.CreateDirectory(DirPath["objects"]);
            DirPath["refs"]=Path.Combine(root, git_folder, "refs");
            Directory.CreateDirectory(DirPath["refs"]);
                DirPath["heads"]=Path.Combine(DirPath["refs"], "heads");
                Directory.CreateDirectory(DirPath["heads"]);
                DirPath["tags"]=Path.Combine(DirPath["refs"], "tags");
                Directory.CreateDirectory(DirPath["tags"]);
        }
        public bool ValidRoot()
        {
            return Root!=null;
        }
        public void AssertValidRoot()
        {
            if (!ValidRoot()) throw new Exception();
        }
        public string PathFromRoot(string path)
        {
            return Path.Combine(Root,(IsBare?"":".git"),path);
        }
        public string PathFromRootUser(string path)
        {
            return Path.Combine(Root,path);
        }
        public string PathFromDir(string dir, string path)
        {
            return Path.Combine(DirPath[dir],path);
        }
        public string PathFromHash(string hash)
        {
            string PTP(string hash_prefix)
            {
                if (hash_prefix.Length < 4) 
                    return null;
                string objd=Path.Combine(DirPath["objects"], hash_prefix.Substring(0,2));
                string rest = hash_prefix.Substring(2);
                string[] objects = Directory.GetFiles(objd, $"{rest}*");
                if (objects.Length!=1) 
                    throw new Exception($"{hash_prefix} ambiguity");
                return objects[0];
            }
            if (hash.Length!=40) 
                try{return PTP(hash);}
                catch(Exception){return null;}
            return Path.Combine(DirPath["objects"],hash.Substring(0,2),hash.Substring(2));
        }
        public string HashFromPath(string path)
        {
            return Path.GetRelativePath(DirPath["objects"],path).Replace($"{Path.DirectorySeparatorChar}","");
        }
        public string RelToRoot(string path)
        {
            return Path.GetRelativePath(Root,path);
        }
    }
}