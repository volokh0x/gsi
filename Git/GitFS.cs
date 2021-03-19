using System;
using System.IO;
using System.Collections.Generic;

namespace gsi
{
    class GitFS
    {
        static (string,bool) FindRoot(string path)
        {
            var path_info=new DirectoryInfo(path);
            while (true)
            {
                string maybe_git=Path.Combine(path_info.FullName, ".git");
                string maybe_config=Path.Combine(path_info.FullName,"config");

                if (Directory.Exists(maybe_git))
                    return (path_info.FullName,false);

                var parent_info = Directory.GetParent(path_info.FullName);
                if (parent_info==null) return (null,false);
                
                if (File.Exists(maybe_config))
                {
                    Config config = new Config(maybe_config);
                    config.ReadConfig();
                    if (config.IsBare()) { return (path_info.FullName,true); }
                }
                path_info=parent_info;
            }
        }
        
        public GitPath gitp;
        public Head head;
        public Index index;
        public Config config;
        public Dictionary<string,Object> Objects = new Dictionary<string, Object>();
        public Dictionary<string,Ref> Refs = new Dictionary<string, Ref>();

        public GitFS(string cwdRelPath)
        {
            // find root, if found then init paths
            (string root, bool is_bare)=GitFS.FindRoot(Path.GetFullPath(cwdRelPath));
            gitp=new GitPath(root,is_bare);

            // inspect git
            string fpath;
            if (!gitp.ValidRoot()) return;
            fpath=gitp.PathFromRoot("HEAD");
            if (File.Exists(fpath))
                head=new Head(fpath, true);
            fpath=gitp.PathFromRoot("index");
            if (File.Exists(fpath))
                index=new Index(fpath, true);
            fpath=gitp.PathFromRoot("config");
            if (File.Exists(fpath))
                config=new Config(fpath, true);
                
            // init refs
            // ...
        }
        public GitFS()
        {
            // left blank
        }
        public void Inspect()
        {

        }
    }
}