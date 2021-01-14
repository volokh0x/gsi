using System;
using System.IO;

namespace gsi
{
    partial class X
    {
        public static string FindObject(string sha1_prefix)
        {
            if (sha1_prefix.Length < 2) 
                throw new ArgumentException();
            string obj_dir = Path.Combine(GitPath.wpath, ".git", "objects", sha1_prefix.Substring(0,2));
            string rest = sha1_prefix.Substring(2);
            string[] objects = Directory.GetFiles(obj_dir, $"{rest}*");
            if (objects.Length>2) 
                throw new Exception();
            return objects[0];
        }
    }
}
