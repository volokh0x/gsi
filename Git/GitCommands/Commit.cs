using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    #nullable enable
    partial class X
    {
        private static string? GetLocalMasterHash()
        {
            try
            {
                return File.ReadAllText(GitPath.heads_master).Trim();
            }
            catch(Exception) {return null;}
        }
        public static string Commit(string msg, string? author=null)  
        {
            List<string> L = new List<string>();
            string tree_sha1 = GitPath.WriteTree();
            string? parent = GetLocalMasterHash();
            if (author==null)
                author=$"{Environment.GetEnvironmentVariable("GIT_AUTHOR_NAME")} <{Environment.GetEnvironmentVariable("GIT_AUTHOR_EMAIL")}>";
            uint timestamp=GetTimeStamp(DateTime.UtcNow);
            int utc_offset = Convert.ToInt32(Math.Floor(TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds));
            string author_time=$"{timestamp} {(utc_offset>0?"+":"-")}{(Math.Abs(utc_offset)/3600).ToString("D2")}{((Math.Abs(utc_offset)/60)%60).ToString("D2")}";

            L.Add($"tree {tree_sha1}");
            if (parent!=null)
                L.Add($"parent {parent}");
            L.Add($"author {author} {author_time}");
            L.Add($"comitter {author} {author_time}");
            L.Add($"");
            L.Add(msg);
            L.Add($"");

            string sha1 = X.HashObject(Encoding.UTF8.GetBytes(string.Join("\n", L)), ObjectType.commit);
            File.WriteAllText(GitPath.heads_master, sha1+"\n");
            Console.WriteLine($"commited to master {sha1}");
            return sha1;
        }
    }
}
