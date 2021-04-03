using System;
using System.IO;
using System.Text.RegularExpressions;

namespace gsi
{
    class MergeMsg
    {
        private GitFS gitfs;
        public string MPath{get=>gitfs.gitp.PathFromRoot("MERGE_MSG");}
        public string Content;

        public MergeMsg(GitFS gitfs, bool read_merge_msg=true)
        {
            this.gitfs=gitfs;
            if (read_merge_msg) ReadMergeMsg();
        }
        public void ReadMergeMsg()
        {
            Content=File.ReadAllText(MPath);
        }
        public void WriteMergeMsg(string content=null)
        {
            if (content!=null) Content=content;
            File.WriteAllText(MPath, Content);
        }
        public void Delete()
        {
            File.Delete(MPath);
        }
    }
}