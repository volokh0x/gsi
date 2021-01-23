using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class X
    {
        private static string? GetLocalMasterHash()
        {
            try
            {
                return File.ReadAllText(GitPath.master).Trim();
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
            var diff = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            uint timestamp=Convert.ToUInt32(Math.Floor(diff.TotalSeconds));
            int utc_offset = Convert.ToInt32(Math.Floor(TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds));
            string author_time=$"{timestamp} {(utc_offset>0?"+":"-")}{Math.Abs(utc_offset)/3600}{(Math.Abs(utc_offset)/60)%60}";
            //     author_time = '{} {}{:02}{:02}'.format(
            //             timestamp,
            //             '+' if utc_offset > 0 else '-',
            //             abs(utc_offset) // 3600,
            //             (abs(utc_offset) // 60) % 60)
            L.Add($"tree {tree_sha1}");
            if (parent!=null)
                L.Add($"parent {parent}");
            L.Add($"author {author} {author_time}");
            L.Add($"comitter {author} {author_time}");
            L.Add($"");
            L.Add(msg);
            L.Add($"");

            string sha1 = X.HashObject(Encoding.UTF8.GetBytes(string.Join("\n", L)), ObjectType.commit);
            File.WriteAllText(GitPath.master, sha1+"\n");
            Console.WriteLine($"commited to master {sha1}");
            return sha1;
        }
        // def commit(message, author=None):
        //     tree = write_tree()
        //     parent = get_local_master_hash()
        //     if author is None:
        //         author = '{} <{}>'.format(
        //                 os.environ['GIT_AUTHOR_NAME'], os.environ['GIT_AUTHOR_EMAIL'])
        //     timestamp = int(time.mktime(time.localtime()))
        //     utc_offset = -time.timezone
        //     author_time = '{} {}{:02}{:02}'.format(
        //             timestamp,
        //             '+' if utc_offset > 0 else '-',
        //             abs(utc_offset) // 3600,
        //             (abs(utc_offset) // 60) % 60)
        //     lines = ['tree ' + tree]
        //     if parent:
        //         lines.append('parent ' + parent)
        //     lines.append('author {} {}'.format(author, author_time))
        //     lines.append('committer {} {}'.format(author, author_time))
        //     lines.append('')
        //     lines.append(message)
        //     lines.append('')
        //     data = '\n'.join(lines).encode()
        //     sha1 = hash_object(data, 'commit')
        //     master_path = os.path.join('.git', 'refs', 'heads', 'master')
        //     write_file(master_path, (sha1 + '\n').encode())
        //     print('committed to master: {:7}'.format(sha1))
        //     return sha1
    }
}
