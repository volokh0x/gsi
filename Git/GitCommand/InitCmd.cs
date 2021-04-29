using System;
using System.IO;

namespace gsi
{
    static partial class GitCommand 
    {
        public static void InitCmd(string rpath, bool bare=false) 
        {
            string root_path=Path.GetFullPath(rpath);
            GitFS gitfs=new GitFS(root_path);
            if (gitfs.gitp.ValidRoot()) 
            {
                Console.WriteLine($"git repository already exists at:");
                Console.WriteLine($"\t{gitfs.gitp.Root}");
                return;
            }

            gitfs=new GitFS();
            gitfs.gitp=new GitPath(root_path, bare);
            Directory.SetCurrentDirectory(gitfs.gitp.Root);

            gitfs.config_set=new ConfigSet();
            gitfs.config_set.config_pr=new Config(gitfs.gitp.PathFromRoot("config"),false);
            gitfs.config_set.config_pr.SetOptionValue("core",null,"bare",(bare?"true":"false"));
            gitfs.config_set.config_pr.WriteConfig();

            gitfs.Refs["heads/master"]=new Ref(gitfs,"heads/master",false);
            gitfs.head=new Head(gitfs,false);
            gitfs.head.SetHead(gitfs.Refs["heads/master"]);
            Console.WriteLine($"initialized empty repository:");
            Console.WriteLine($"✔ {gitfs.gitp.Root}");
        }
    }
}