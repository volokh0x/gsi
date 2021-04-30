using System;
using System.IO;
using System.Text.RegularExpressions;

namespace gsi
{
    class Ref
    {
        public static (bool,string) GetIsDetachedAndHash(GitFS gitfs, string ref_or_hash)
        {
            string path = gitfs.gitp.PathFromHash(ref_or_hash);
            if (path!=null) return (true,gitfs.gitp.HashFromPath(path));
            string terminal_ref=TerminalRef(gitfs,ref_or_hash);
            return (false,gitfs.Refs[terminal_ref].Hash);
        }
        public static string TerminalRef(GitFS gitfs, string iref)
        {
            if (iref=="HEAD" && gitfs.head.IsDetached) return gitfs.head.Content;
            if (IsFullRef(iref))
            {
                if (iref.StartsWith("refs/heads/")) iref=iref.Substring(11);
                if (iref.StartsWith("refs/remotes/")) iref=iref.Substring(13);
            }
            return $"heads/{iref}";
        }
        public static bool IsFullRef(string iref)
        {
            return iref!=null && 
            (
                   Regex.IsMatch(iref,"^refs/heads/[A-Za-z-]+$")
                || Regex.IsMatch(iref,"^refs/remotes/[A-Za-z-]+/[A-Za-z-]+$")
                || iref=="HEAD"
                || iref=="MERGE_HEAD"
            );
        }

        private GitFS gitfs;
        public string Name; 
        public string RPath {get => gitfs.gitp.PathFromDir("refs", Name);}
        public string Hash;

        public Ref(GitFS gitfs, string Name, bool read_ref=true)
        {
            this.gitfs=gitfs;
            this.Name=Name;
            if (read_ref) ReadRef();
        }
        public string ReadRef()
        {
            Hash=Regex.Replace(File.ReadAllText(RPath), @"\s+", string.Empty);
            return Hash;
        }
        public void SetRef(string hash)
        {
            Hash=hash;
            File.WriteAllText(RPath, $"{Hash}\n");
        }
        public void Delete()
        {
            File.Delete(RPath);
        }
    }
}