using System;
using System.IO;
using System.Text.RegularExpressions;

namespace gsi
{
    class MergeHead
    {
        private GitFS gitfs;
        public string MPath{get=>gitfs.gitp.PathFromRoot("MERGE_HEAD");}
        public string Hash;

        public MergeHead(GitFS gitfs, bool read_merge_head=true)
        {
            this.gitfs=gitfs;
            if (read_merge_head) ReadMergeHead();
        }
        public string ReadMergeHead()
        {
            Hash=Regex.Replace(File.ReadAllText(MPath), @"\s+", string.Empty);
            return Hash;
        }
        public void SetMergeHead(string hash)
        {
            Hash=hash;
            File.WriteAllText(MPath, $"{hash}\n");
        }
        public void Delete()
        {
            File.Delete(MPath);
        }
    }
}