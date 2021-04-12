using System;
using System.IO;
using System.Linq;
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
        public FetchHead fetch_head;
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
            fpath=gitp.PathFromRoot("FETCH_HEAD");
            if (File.Exists(fpath))
                fetch_head=new FetchHead(this);
            fpath=gitp.PathFromRoot("MERGE_MSG");
            if (File.Exists(fpath))
                merge_msg=new MergeMsg(this);
            fpath=gitp.PathFromRoot("index");
            if (File.Exists(fpath))
                index=new Index(this);
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
        public Dictionary<string,string> PToH()
        {
            Dictionary<string,string> _PToH(string dir)
            {
                var D = new Dictionary<string,string>();
                foreach(var file in Directory.EnumerateFiles(dir))
                {
                    string path = Path.Combine(dir,new FileInfo(file).Name);
                    D.Add(gitp.RelToRoot(path), new Blob(this,path,true).HashBlob());
                }
                foreach(var d in Directory.EnumerateDirectories(dir))
                {
                    if (new DirectoryInfo(d).Name!=".git")
                        foreach(var el in _PToH(d))
                            D.Add(el.Key,el.Value);
                }
                return D;
            }
            return _PToH(Environment.CurrentDirectory);
        }
        public void ApplyDiff(Dictionary<string,FileDiffInfo> diffs)
        {
            foreach(var el in diffs)
            {
                var path = el.Key;
                var diff = el.Value;
                switch(diff.Status)
                {
                    case FileDiffStatus.ADD:
                        string hash = diff.Receiver!=null?diff.Receiver:diff.Giver;
                        File.WriteAllText(path,new Blob(this,hash,false).Content);
                        break;
                    case FileDiffStatus.CONFLICT:
                        File.WriteAllLines
                        (
                            path,
                            ComposeConflict(
                                new Blob(this,diff.Receiver,false).Content.Split().ToList(),
                                new Blob(this,diff.Giver,false).Content.Split().ToList())
                        );
                        break;
                    case FileDiffStatus.MODIFY:
                        File.WriteAllText(path, new Blob(this,diff.Giver,false).Content);
                        break;
                    case FileDiffStatus.DELETE:
                        File.Delete(path);
                        break;
                }
            }
            // delete empty dirs !!!
        }
        private List<string> ComposeConflict(List<string>  text1, List<string>  text2)
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            text1.AddRange(text2);
            return text1;
        }
    }
}