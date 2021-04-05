using System;
using System.IO;
using System.Text.RegularExpressions;

namespace gsi
{
    class Head 
    {
        private GitFS gitfs;
        public string HPath{get=>gitfs.gitp.PathFromRoot("HEAD");}
        public bool IsDetached;
        public string Content;
        public string Hash{get=>GetHash();}
        
        public Head(GitFS gitfs, bool read_head=true)
        {
            this.gitfs=gitfs;
            if (read_head) ReadHead();
        }
        public void ReadHead()
        {
            string content = Regex.Replace(File.ReadAllText(HPath), @"\s+", string.Empty);
            if (content.StartsWith("ref:"))
            {
                IsDetached=false;
                Content=new Regex(".*refs/(.*)$").Match(content).Groups[1].Value;
            }
            else
            {
                IsDetached=true;
                Content=content;
            }
        }
        public void SetHead(string hash, bool to_detached=false)
        {
            if (!to_detached)
            {
                if (IsDetached) 
                    throw new Exception("not pointing at any branch");
                gitfs.Refs[Content].SetRef(hash);
            }
            else
            {
                IsDetached=true;
                Content=hash;
                File.WriteAllText(HPath, $"{hash}\n");
            }
        }
        public void SetHead(Ref iref)
        {
            IsDetached=false;
            Content=iref.Name;
            File.WriteAllText(HPath, $"ref: refs/{iref.Name}\n");
        }
        private string GetHash()
        {
            if (!IsDetached)
                if (gitfs.Refs.ContainsKey(Content)) 
                    return gitfs.Refs[Content].Hash;
                else return null;
            else
                return Content;
        }
    }
}