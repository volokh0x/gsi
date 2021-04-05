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
                    if (config.IsBare) { return (path_info.FullName,true); }
                }
                path_info=parent_info;
            }
        }
        
        public GitPath gitp;
        public Head head;
        public MergeHead merge_head;
        public MergeMsg merge_msg;
        public Index index;
        public ConfigSet config_set;
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
                head=new Head(this);
            fpath=gitp.PathFromRoot("MERGE_HEAD");
            if (File.Exists(fpath))
                merge_head=new MergeHead(this);
            fpath=gitp.PathFromRoot("MERGE_MSG");
            if (File.Exists(fpath))
                merge_msg=new MergeMsg(this);
            fpath=gitp.PathFromRoot("index");
            if (File.Exists(fpath))
                index=new Index(fpath);
            config_set=new ConfigSet();
            fpath=gitp.PathFromRoot("config");
            if (File.Exists(fpath))
                config_set.config_pr=new Config(fpath);
            fpath=$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.gitconfig";
            if (File.Exists(fpath))
                config_set.config_usr=new Config(fpath);
            fpath="/etc/gitconfig";
            if (File.Exists(fpath))
                config_set.config_glbl=new Config(fpath);
            
            // init all exisitng refs in repo
            foreach (var reff in Directory.GetFiles(gitp.DirPath["tags"]))
            {
                string fname = Path.GetFileName(reff);
                string ref_name = $"tags/{fname}";
                Refs[ref_name ]=new Ref(this, ref_name);
            }
            bool master_found=false;
            foreach (var reff in Directory.GetFiles(gitp.DirPath["heads"]))
            {
                string fname = Path.GetFileName(reff);
                string ref_name = $"heads/{fname}";
                Refs[ref_name ]=new Ref(this, ref_name);
                if (fname=="master") master_found=true;
            }
            if (!master_found) 
                Refs["heads/master"]=new Ref(this,"heads/master",false);
        }
        public GitFS()
        {
            // left blank
        }
        public List<string> GetParentCommits()
        {
            var L = new List<string>();
            if (head.Hash!=null)
                L.Add(head.Hash);
            if (merge_head!=null)
                L.Add(merge_head.Hash);
            return L;
        }
    }
}