using System;
using System.Linq;
using System.Collections.Generic;

namespace gsi
{
    enum FileDiffStatus
    {
        ADD,
        MODIFY,
        DELETE,
        SAME,
        CONFLICT
    }
    struct FileDiffInfo
    {
        public FileDiffStatus Status;
        public string Receiver;
        public string Giver;
        public string Base;
        public override string ToString()
        {
            return $"stat {Status.ToString()} receiver {Receiver} giver {Giver} base {Base}";
        }
    }
    static class DiffCalc
    {
        public static Dictionary<string, FileDiffInfo> Diff(GitFS gitfs, string hash1=null, string hash2=null)
        {
            var a = hash1==null? gitfs.index.PToH() : Tree.PToH(gitfs,new Commit(gitfs,hash1).Content.tree_hash);
            var b = hash2==null? gitfs.PToH() : Tree.PToH(gitfs,new Commit(gitfs,hash2).Content.tree_hash);
            return DiffInfos(a,b,null);
        }
        private static Dictionary<string, FileDiffInfo> DiffInfos(
            Dictionary<string,string> receiver, Dictionary<string,string> giver, Dictionary<string,string> bbase=null)
        {
            FileDiffStatus GetFileStatus(string receiver, string giver, string bbase)
            {
                bool re = receiver!=null;
                bool gi = giver!=null;
                bool bb = bbase!=null;

                if (re && gi && receiver!=giver)
                {
                    if (receiver!=bbase && giver!=bbase) return FileDiffStatus.CONFLICT;
                    return FileDiffStatus.MODIFY;
                }
                else if (receiver==giver)
                {
                    return FileDiffStatus.SAME;
                }
                else if (
                       (gi && !re && !bb) 
                    || (re && !gi && !bb))
                    return FileDiffStatus.ADD;
                return FileDiffStatus.DELETE;
            }
            if (bbase==null) bbase=receiver;
            var paths = receiver.Keys.ToList();
            paths=paths.Union(giver.Keys).ToList();
            paths=paths.Union(bbase.Keys).ToList();
            var D = new Dictionary<string,FileDiffInfo>();
            foreach(var path in paths)
            {
                string hash1=receiver.GetValueOrDefault(path,null);
                string hash2=giver.GetValueOrDefault(path,null);
                string hash3=bbase.GetValueOrDefault(path,null);
                D.Add
                (
                    path,
                    new FileDiffInfo()
                    {
                        Status=GetFileStatus(hash1,hash2,hash3),
                        Receiver=hash1,
                        Giver=hash2,
                        Base=hash3
                    }
                );
            }
            return D;
        }
        public static List<string> CommitWouldOverwrite(GitFS gitfs, string hash)
        {
            string head_hash = gitfs.head.Hash;
            var a = DiffCalc.Diff(gitfs,head_hash).Where(el=>el.Value.Status!=FileDiffStatus.SAME).Select(el=>el.Key).ToList();
            var b = DiffCalc.Diff(gitfs,head_hash,hash).Where(el=>el.Value.Status!=FileDiffStatus.SAME).Select(el=>el.Key).ToList();
            return a.Intersect(b).ToList();
        }
        public static List<string> AddedOrModified(GitFS gitfs)
        {
            var head_PToH = gitfs.head.Hash!=null?Tree.PToH(gitfs,new Commit(gitfs,gitfs.head.Hash).Content.tree_hash):new Dictionary<string, string>();
            return DiffCalc.DiffInfos(head_PToH,gitfs.index.PToH())
                .Where(el=>el.Value.Status!=FileDiffStatus.SAME && el.Value.Status!=FileDiffStatus.DELETE)
                .Select(el=>el.Key)
                .ToList();
        }
    }
}