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
        MODIFYre,
        MODIFYgi,
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
                gitfs.ReadObjsRecursively(hash3,Num.BASE);
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
                    if (giver!=bbase) return FileDiffStatus.MODIFYgi;
                    if (receiver!=bbase) return FileDiffStatus.MODIFYre;
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
            (var lmer,var lch,var lnew,var ldel)=gitfs.TrackWorkingCopy();
            return lmer.Union(lch).Union(lnew).Union(ldel).ToList();
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
        public static List<string> ComposeConflict(string[] text1, string ref1, string[] text2, string ref2)
        {
            List<(int,int)> GetIndexes(int l, int r)
            {
                if (l==-1 || r==-1) return new List<(int,int)>();
                if (text1[l]==text2[r])
                {
                    List<(int,int)> L = GetIndexes(l-1,r-1);
                    L.Add((l,r));
                    return L;
                }
                else
                {
                    List<(int,int)> L1  = GetIndexes(l-1,r);
                    List<(int,int)> L2  = GetIndexes(l,r-1);
                    if (L1.Count>L2.Count) return L1;
                    else return L2;
                }
            }
            var text=new List<string>();
            void Mark(int st1, int len1, int st2, int len2)
            {
                if (len1==0 && len2==0) return;
                int end1=st1+len1, end2=st2+len2;
                text.Add($"<<<<<<< {ref1}");
                for (int i=st1; i<end1; i++) text.Add(text1[i]);
                text.Add("=======");
                for (int i=st2; i<end2; i++) text.Add(text2[i]);
                text.Add($">>>>>>> {ref2}");
            }
            List<(int,int)> L =GetIndexes(text1.Length-1,text2.Length-1);
            L.Add((text1.Length,text2.Length));
            int prevl=-1, prevr=-1;

            foreach((int l, int r) in L)
            {
                int st1=prevl+1, st2=prevr+1;
                int lenl=l-prevl-1, lenr=r-prevr-1;
                Mark(st1,lenl,st2,lenr);
                if (l<text1.Length) text.Add(text1[l]);
                prevl=l; prevr=r;
            }
            return text;
        }
    }
}