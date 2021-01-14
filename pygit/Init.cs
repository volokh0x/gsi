using System;
using System.IO;

namespace gsi
{
    partial class X 
    {
        private static void MakeGitDir(string repo)
        {
            Directory.CreateDirectory(Path.Combine(repo, ".git", "refs", "heads"));
            Directory.CreateDirectory(Path.Combine(repo, ".git", "refs", "tags"));
            Directory.CreateDirectory(Path.Combine(repo, ".git", "objects"));
            File.WriteAllText(Path.Combine(repo, ".git", "HEAD"), 
                $"ref: {Path.Combine("refs", "heads", "master")}");
        }
        private static void Reinit(string repo)
        {
            Directory.Delete(Path.Combine(repo, ".git"), true);
            MakeGitDir(repo);
            Console.WriteLine($"reinitialized empty repository: {repo}");
        }
        public static void Init(string repo) 
        {
            if (Directory.Exists(Path.Combine(repo, ".git")))
                Reinit(repo);
            else
            {
                MakeGitDir(repo);
                Console.WriteLine($"initialized empty repository: {repo}");
            }    
        }
    }
}