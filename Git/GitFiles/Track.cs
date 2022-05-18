using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Mono.Unix;

namespace gsi
{
    class Track
    {
        public enum Status
        {
            INCLUDED,
            EXLUDED,
            TMP
        }
        private GitFS gitfs;
        public string TrackPath {get => gitfs.gitp.PathFromRootUser(".track");}
        public Dictionary<string,Status> Entries= new Dictionary<string, Status>();
        public List<string> included {get=>_Included();}
        public List<string> excluded {get=>_Excluded();}
        public List<string> tmp { get => _Tmp(); }

        public Track(GitFS gitfs, bool read_track=true)
        {
            this.gitfs=gitfs;
            if (read_track) ReadTrack();
        }
        public void ReadTrack()
        {
            foreach(var line in File.ReadAllLines(TrackPath))
            {
                var mas = line.Split();
                if (mas[0]==string.Empty) continue;
                if (mas.Length!=2) throw new Exception(".track file is invalide");
                if (mas[0]=="+")
                    Entries.Add(mas[1], Status.INCLUDED);
                else if (mas[0]=="-")
                    Entries.Add(mas[1], Status.EXLUDED);
                else if (mas[0]=="#")
                    Entries.Add(mas[1], Status.TMP);
                else
                    throw new Exception(".track file is invalide");
            }
        }
        private List<string> Stringify()
        {
            var L = new List<string>();

            foreach (var path in Entries.Keys.OrderBy(path => path).ToList())
            {
                string symb="";
                if (Entries[path]==Status.INCLUDED)
                    symb = "+";
                if (Entries[path]==Status.EXLUDED)
                    symb = "-";
                if (Entries[path]==Status.TMP)
                    symb = "#";
                L.Add($"{symb} {path}");
            }
            return L;
        }
        public void WriteTrack()
        {
            File.WriteAllLines(TrackPath,Stringify());
        }
        public void SetEntry(string path, Status st=Status.EXLUDED)
        {
            Entries[path]=st;
        }
        public void SetEntries(List<string> paths, Status st=Status.EXLUDED)
        {
            foreach(var path in paths)
                SetEntry(path,st);
        }
        public void RemoveEntry(string path)
        {
            Entries.Remove(path);
        }
        public bool IsFilePresent(string path)
        {
            return Entries.Keys.Contains(path);
        }
        private List<string> _Included()
        {
            List<string> L=new List<string>();
            foreach(var el in Entries)
                if (el.Value==Status.INCLUDED) L.Add(el.Key);
            return L;
        }
        private List<string> _Excluded()
        {
            List<string> L=new List<string>();
            foreach(var el in Entries)
                if (el.Value==Status.EXLUDED) L.Add(el.Key);
            return L;
        }
        private List<string> _Tmp() {
            List<string> L=new List<string>();
            foreach(var el in Entries)
                if (el.Value==Status.TMP) L.Add(el.Key);
            return L;
        }
    }
}