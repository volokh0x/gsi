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
        public Track track;
        public ConfigSet config_set;
        public Dictionary<string,Object> Objs = new Dictionary<string, Object>();
        public Dictionary<string,string>[] PToH = new Dictionary<string, string>[3];
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
            fpath=gitp.PathFromRootUser(".track");
            if (File.Exists(fpath))
                track=new Track(this);
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
        public string WriteTreeGraph()
        {
            var tree = new Tree(this, new List<IndexEntry>(index.Entries), Environment.CurrentDirectory);
            return tree.WriteTree();
        }
        public string CreateCommit(GitFS gitfs, string tree_hash, string message)
        {
            var commit = new Commit(gitfs, tree_hash, message);
            return commit.WriteCommit();
        }
        public (List<string>,List<string>,List<string>,List<string>) TrackWorkingCopy()
        {
            (var _lmer, var _lch,var _lnew,var _ldel)=index.GetStatus();
            var lch=new List<string>();
            var lnew=new List<string>();
            var ldel=new List<string>();
            foreach(var path in _lch)
            {
                if (!track.included.Contains(path)) continue;
                if (path!=".track") 
                    lch.Add(path);
            }
            foreach(var path in _lnew)
            {
                if (track.excluded.Contains(path)) continue;
                if (path!=".track") 
                {
                    track.SetEntry(path,true);
                    lnew.Add(path);
                }    
            }
            foreach(var path in _ldel)
            {
                if (track.excluded.Contains(path)) continue;
                if (path!=".track")
                {
                    track.RemoveEntry(path);
                    ldel.Add(path);
                }      
            }
            track.WriteTrack(); 
            return (_lmer,lch,lnew,ldel);
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
            PToH[num]=new Dictionary<string, string>();
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
            PToH[num]=new Dictionary<string, string>();
            RWCR(num,null);
        }
        public void ApplyDiff(Dictionary<string,FileDiffStatus> diffs, string ref1, string ref2)
        {
            foreach(var el in diffs)
            {
                var path = el.Key;
                var status = el.Value;

                string hash1=null, hash2=null;
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
                string pardir=Path.GetDirectoryName(path);
                if (pardir!=string.Empty)
                    Directory.CreateDirectory(pardir);
                switch(status)
                {
                    case FileDiffStatus.ADD:
                        if (blob1!=null)
                            File.WriteAllText(path,blob1.Content);
                        if (blob2!=null)
                            File.WriteAllText(path,blob2.Content);
                        break;
                    case FileDiffStatus.CONFLICT:
                        File.WriteAllLines(path,
                            DiffCalc.ComposeConflict(blob1.Content.Split("\n"),ref1,blob2.Content.Split("\n"),ref2));
                        break;
                    case FileDiffStatus.MODIFYgi:
                        File.WriteAllText(path, blob2.Content);
                        break;
                    case FileDiffStatus.MODIFYre:
                        File.WriteAllText(path, blob1.Content);
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
        public List<string> GetFiles(List<string> paths)
        {
            List<string> fpaths=new List<string>();
            foreach(var path in paths)
                if (File.Exists(path))
                    fpaths.Add(path);
                else if (Directory.Exists(path))
                    fpaths.AddRange(Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories));
                else 
                    throw new Exception($"{path} was not found");
            return fpaths;
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
        public string GetBaseCommitHash(string hash1, string hash2)
        {
            int RCR(string start_hash)
            {
                int depth=-1;
                var S=new Stack<string>();
                S.Push(start_hash);
                while (S.Count!=0)
                {
                    string hash = S.Pop();
                    var commit = new Commit(this,hash);
                    Objs[hash]=commit;
                    if (commit.Content.parent_hashes.Count!=0)
                        S.Push(commit.Content.parent_hashes[0]);
                    depth+=1;
                }
                return depth;
            }

            int h1 = RCR(hash1); 
            int h2 = RCR(hash2);

            while (h1!=h2)
            {
                if (h1>h2)
                {
                    hash1=((Commit)Objs[hash1]).Content.parent_hashes[0];
                    h1-=1;
                }
                else
                {
                    hash2=((Commit)Objs[hash2]).Content.parent_hashes[0];
                    h2-=1; 
                }
            }

            while (hash1!=hash2)
            {
                hash1=((Commit)Objs[hash1]).Content.parent_hashes[0];
                hash2=((Commit)Objs[hash2]).Content.parent_hashes[0];
            }

            return hash1;
        }
    }
}