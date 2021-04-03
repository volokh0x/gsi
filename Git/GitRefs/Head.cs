using System;
using System.IO;
using System.Text.RegularExpressions;

namespace gsi
{
    class Head 
    {
        private GitFS gitfs;
        public string HPath{get=>gitfs.gitp.PathFromRoot("HEAD");}
        public bool ToBranch;
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
                ToBranch=true;
                Content=new Regex(".*refs/(.*)$").Match(content).Groups[1].Value;
            }
            else
            {
                ToBranch=false;
                Content=content;
            }
        }
        public void SetHead(string hash, bool effect_branch=true)
        {
            if (effect_branch)
            {
                if (!ToBranch) 
                    throw new Exception("not pointing at any branch");
                gitfs.Refs[Content].SetRef(hash);
            }
            else
            {
                ToBranch=false;
                Content=hash;
                File.WriteAllText(HPath, $"{hash}\n");
            }
        }
        public void SetHead(Ref iref)
        {
            ToBranch=true;
            Content=iref.Name;
            File.WriteAllText(HPath, $"ref: refs/{iref.Name}\n");
        }
        private string GetHash()
        {
            if (ToBranch)
                if (gitfs.Refs.ContainsKey(Content)) return gitfs.Refs[Content].Hash;
                else return null;
            else
                return Content;
        }
    }
}