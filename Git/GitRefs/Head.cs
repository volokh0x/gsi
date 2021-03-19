using System;
using System.IO;
using System.Text.RegularExpressions;

namespace gsi
{
    class Head 
    {
        public string HeadPath;
        public bool ToBranch;
        public string Content;
        
        public Head(string path, bool read_head=false)
        {
            HeadPath=path;
            if (read_head) ReadHead();
        }
        public void ReadHead()
        {
            string content = Regex.Replace(File.ReadAllText(HeadPath), @"\s+", string.Empty);
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
        public void SetHead(string hash)
        {
            ToBranch=false;
            Content=hash;
            File.WriteAllText(HeadPath, $"{hash}\n");
        }
        public void SetHead(Ref iref)
        {
            ToBranch=true;
            Content=iref.Name;
            File.WriteAllText(HeadPath, $"ref: refs/{iref.Name}\n");
        }
    }
}