using System;
using System.IO;

namespace gsi
{
    partial class X
    {
        public static string FindObject(string prefix)
        {
            if (prefix.Length < 2) 
                throw new ArgumentException();
            string obj_dir=Path.Combine(GitPath.DirFullPath["objects"], prefix.Substring(0,2));
            string rest = prefix.Substring(2);
            string[] objects = Directory.GetFiles(obj_dir, $"{rest}*");
            
            if (objects.Length>2) 
                throw new Exception();
            return objects[0];
        }
    }
}
