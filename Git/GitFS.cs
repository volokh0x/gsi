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
        public Dictionary<string,Object> Objs = new Dictionary<string, Object>();
        public Dictionary<string,string>[] PToH = new Dictionary<string, string>[3] {new Dictionary<string, string>(), new Dictionary<string, string>(),new Dictionary<string, string>()};
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
        public void ReadObjsRecursively(string hash,int num)
        {
            void ROR(string hash,int num,List<string> paths)
            {
                string path;
                if (paths==null){paths=new List<string>(); path="";}
                else {path=Path.Combine(paths.ToArray());}

                string path_from_hash = gitp.PathFromHash(hash);
                byte[] data; ObjectType objt;

                if (!Objs.ContainsKey(hash))
                    (data, objt)=Object.ReadObject(path_from_hash);
                else
                {
                    if (Objs[hash] is Blob) objt=ObjectType.blob;
                    else if (Objs[hash] is Tree) objt=ObjectType.tree;
                    else objt=ObjectType.commit;
                }
                if (objt==ObjectType.blob)
                {
                    var blob = new Blob(this,path_from_hash,false);
                    Objs[hash]=blob; 
                    PToH[num][path]=hash; 
                }  
                else if (objt==ObjectType.tree)
                {
                    var tree = new Tree(this,hash);
                    Objs[hash]=tree;
                    foreach(var te in tree.Entries)
                    {
                        paths.Add(te.name);
                        ROR(te.hash,num,paths);
                        paths.RemoveAt(paths.Count-1);
                    }     
                }
                else if (objt==ObjectType.commit)
                {
                    var commit = new Commit(this,hash);
                    Objs[hash]=commit;
                    ROR(commit.Content.tree_hash,num,null);
                }
            }
            ROR(hash,num,null);
        }
        public void ReadWorkingCopyRecursively(int num)
        {
            void RWCR(int num,string path)
            {
                if (path==null) path=Environment.CurrentDirectory;
                foreach(var file in Directory.EnumerateFiles(path))
                {
                    string fpath = Path.Combine(path,new FileInfo(file).Name);
                    var blob = new Blob(this,fpath,true);
                    string hash = blob.HashBlob();
                    Objs[hash]=blob;
                    PToH[num][gitp.RelToRoot(fpath)]=hash;
                }
                foreach(var p in Directory.EnumerateDirectories(path))
                    if (new DirectoryInfo(p).Name!=".git")
                        RWCR(num,p);
            }
            RWCR(num,null);
        }
        public void ApplyDiff(Dictionary<string,FileDiffStatus> diffs)
        {
            foreach(var el in diffs)
            {
                Console.WriteLine($"{el.Key} {el.Value.ToString()}");
                var path = el.Key;
                var status = el.Value;
                
                string hash1, hash2;
                Blob blob1=null, blob2=null;

                if (PToH[Num.RECEIVER].ContainsKey(path))
                {
                    hash1=PToH[Num.RECEIVER][path];
                    blob1=(Blob)Objs[hash1];
                }
                if (PToH[Num.GIVER].ContainsKey(path))
                {
                    hash2=PToH[Num.GIVER][path];
                    blob2=(Blob)Objs[hash2];
                }
                switch(status)
                {
                    case FileDiffStatus.ADD:
                        File.WriteAllText(path,blob1.Content);
                        break;
                    case FileDiffStatus.CONFLICT:
                        File.WriteAllLines(path,
                            DiffCalc.ComposeConflict(blob1.Content.Split().ToList(),blob2.Content.Split().ToList()));
                        break;
                    case FileDiffStatus.MODIFY:
                        File.WriteAllText(path, blob2.Content);
                        break;
                    case FileDiffStatus.DELETE:
                        File.Delete(path);
                        break;
                }
            }
            DeleteEmptyDirs();
        }
        public void DeleteEmptyDirs()
        {
            void DED(string path)
            {
                foreach (var directory in Directory.GetDirectories(path))
                {
                    if (new DirectoryInfo(directory).Name==".git") 
                        continue;
                    DED(directory);
                    if (Directory.GetFiles(directory).Length == 0 && 
                        Directory.GetDirectories(directory).Length == 0)
                    {
                        Directory.Delete(directory, false);
                    }
                }
            }
            DED(gitp.Root);
        }
    }
}