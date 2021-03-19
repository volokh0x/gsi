using System;
using System.IO;

namespace gsi
{
    static partial class GitCommand 
    {
        public static void Init(string rpath, bool bare=false) 
        {
            string root_path=Path.GetFullPath(rpath);
            GitFS gitfs=new GitFS(root_path);
            if (gitfs.gitp.ValidRoot()) 
            {
                Console.WriteLine($"git repository already exists at: {gitfs.gitp.Root}");
                Console.WriteLine($"aborted ...");
                return;
            }

            gitfs=new GitFS();
            gitfs.gitp=new GitPath(root_path, bare);
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            gitfs.config=new Config(gitfs.gitp.PathFromRoot("config"));
            gitfs.config.SetOptionValue("core",null,"bare",(bare?"true":"false"));
            gitfs.config.WriteConfig();

            gitfs.Refs["heads/master"]=new Ref(gitfs.gitp.PathFromDir("heads","master"));
            gitfs.head=new Head(gitfs.gitp.PathFromRoot("HEAD"));
            gitfs.head.SetHead(gitfs.Refs["heads/master"]);
            Console.WriteLine($"initialized empty repository: {gitfs.gitp.Root}");
        }
    }
}