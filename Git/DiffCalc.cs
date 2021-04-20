using System;
using System.Linq;
using System.Collections.Generic;

namespace gsi
{
    static class Num
    {
        public const int RECEIVER=0;
        public const int GIVER=1;
        public const int BASE=2;
    }
    enum FileDiffStatus
    {
        ADD,
        MODIFY,
        DELETE,
        SAME,
        CONFLICT
    }
    static class DiffCalc
    {
        public static Dictionary<string, FileDiffStatus> Diff(GitFS gitfs, string hash1=null, string hash2=null, string hash3=null)
        {
            
            Dictionary<string,string> a, b, c;
            if (hash1!=null)
                gitfs.ReadObjsRecursively(hash1,Num.RECEIVER);
            else
                gitfs.PToH[Num.RECEIVER]=gitfs.index.PToH();
            a=gitfs.PToH[Num.RECEIVER];
            if (hash2!=null)
                gitfs.ReadObjsRecursively(hash2,Num.GIVER);
            else                
                gitfs.ReadWorkingCopyRecursively(Num.GIVER);
            b=gitfs.PToH[Num.GIVER];
            if (hash3!=null)
                gitfs.ReadObjsRecursively(hash2,Num.BASE);
            c=hash3!=null?gitfs.PToH[Num.BASE]:null;
            return DiffInfos(a,b,c);
        }
        private static Dictionary<string, FileDiffStatus> DiffInfos(
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
                    return FileDiffStatus.SAME;
                else if (
                       (!bb && re && !gi)
                    || (!bb && gi && !re))
                    return FileDiffStatus.ADD;
                else if (
                       (bb && re && !gi)
                    || (bb && gi && !re))
                    return FileDiffStatus.DELETE;
                // else if (gi)
                //     return FileDiffStatus.ADD;
                // else if (!gi)
                //     return FileDiffStatus.DELETE;
                throw new Exception("something went wrong when comparing hashes");
            }
            if (bbase==null) bbase=receiver;
            var paths = receiver.Keys.ToList();
            paths=paths.Union(giver.Keys).ToList();
            paths=paths.Union(bbase.Keys).ToList();
            var D = new Dictionary<string,FileDiffStatus>();
            foreach(var path in paths)
            {
                string hash1, hash2, hash3;
                receiver.TryGetValue(path,out hash1);
                giver.TryGetValue(path,out hash2);
                bbase.TryGetValue(path,out hash3);
                D.Add(path, GetFileStatus(hash1,hash2,hash3));
            }
            return D;
        }
        public static List<string> CommitWouldOverwrite(GitFS gitfs)
        {
            string head_hash = gitfs.head.Hash;
            // var a = DiffCalc.Diff(gitfs,head_hash).Where(el=>el.Value!=FileDiffStatus.SAME).Select(el=>el.Key).ToList();
            // var b = DiffCalc.Diff(gitfs,head_hash,hash).Where(el=>el.Value!=FileDiffStatus.SAME).Select(el=>el.Key).ToList();
            return DiffCalc.Diff(gitfs,head_hash).Where(el=>el.Value!=FileDiffStatus.SAME).Select(el=>el.Key).ToList();
        }
        public static List<string> AddedOrModified(GitFS gitfs)
        {
            Dictionary<string,string> a,b;

            gitfs.PToH[Num.RECEIVER]=gitfs.index.PToH();
            a=gitfs.PToH[Num.RECEIVER];

            if (gitfs.head.Hash!=null)
                gitfs.ReadObjsRecursively(gitfs.head.Hash,Num.GIVER);
            else 
                gitfs.PToH[Num.GIVER]=new Dictionary<string, string>();
            b=gitfs.PToH[Num.GIVER];

            return DiffCalc.DiffInfos(a,b)
                .Where(el=>el.Value!=FileDiffStatus.SAME && el.Value!=FileDiffStatus.DELETE)
                .Select(el=>el.Key)
                .ToList();
        }
        public static List<string> ComposeConflict(List<string> text1, List<string> text2)
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            text1.Insert(0,">>>>>>> a");
            text1.Insert(text1.Count-1,"<<<<<<<< b");
            text1.AddRange(text2);
            return text1;
        }
    }
}