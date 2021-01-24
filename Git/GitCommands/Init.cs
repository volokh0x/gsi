using System;
using System.IO;

namespace gsi
{
    partial class X 
    {
        private static void CreateGitStructure(string rpath)
        {
            Directory.CreateDirectory(GitPath.DirFullPath["objects"]);
            Directory.CreateDirectory(GitPath.DirFullPath["heads"]);
            Directory.CreateDirectory(GitPath.DirFullPath["tags"]);
            File.WriteAllText(GitPath.HEAD, $"ref: {Path.GetRelativePath(GitPath.DirFullPath[".git"], GitPath.heads_master)}");
        }
        public static void Init(string rpath) 
        {
            GitPath.GitPathInitRoot(rpath);
            if (Directory.Exists(GitPath.DirFullPath[".git"]))
            {
                Directory.Delete(GitPath.DirFullPath[".git"], true);
                CreateGitStructure(rpath);
                Console.WriteLine($"reinitialized non-empty repository: {rpath}");
            }
            else
            {
                CreateGitStructure(rpath);
                Console.WriteLine($"initialized empty repository: {rpath}");
            }    
        }
    }
}