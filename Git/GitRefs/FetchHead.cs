using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace gsi
{
    class FetchHead
    {
        private GitFS gitfs;
        public string FPath{get=>gitfs.gitp.PathFromRoot("FETCH_HEAD");}
        public string[] Content;

        public FetchHead(GitFS gitfs, bool read_fetch_head=true)
        {
            this.gitfs=gitfs;
            if (read_fetch_head) ReadFetchHead();
        }
        public void ReadFetchHead()
        {
            Content=File.ReadAllLines(FPath);
        }
        // public void SetFetchHead()
        // {

        //     // File.WriteAllText();
        // }
        public string FetchHeadBranchToMerge(string branch_name)
        {
            return Content
                .Where(line=>Regex.IsMatch(line, $"^.+ branch {branch_name} of"))
                .Select(line=>new Regex("^([^ ]+) ").Match(line).Groups[1].Value).First();
        }
        public void Delete()
        {
            File.Delete(FPath);
        }
    }
}