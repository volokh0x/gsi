using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
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
        public void SetMergeMsg(string ref_name, Dictionary<string,FileDiffStatus> conflicts)
        {
            var msg=new StringBuilder();
            msg.AppendLine($"Merge {ref_name} into {gitfs.head.Branch}");
            if (conflicts.Count!=0)
            {
                msg.AppendLine("Conflicts:");
                foreach(var path in conflicts.Keys)
                    msg.AppendLine($"  {path}");
            }
            Content=msg.ToString();
            File.WriteAllText(MPath, Content);
        }
        public void Delete()
        {
            File.Delete(MPath);
        }
    }
}