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
        private GitFS gitfs;
        public string TrackPath {get => gitfs.gitp.PathFromRootUser(".track");}
        public Dictionary<string,bool> Entries= new Dictionary<string, bool>();
        public List<string> included {get=>_Included();}
        public List<string> excluded {get=>_Excluded();}

        public Track(GitFS gitfs, bool read_track=true)
        {
            this.gitfs=gitfs;
            if (read_track) ReadTrack();
        }
        public void ReadTrack()  
        {
            foreach(var line in File.ReadAllLines(TrackPath))
            {
                if (line.StartsWith("#")) continue;
                var mas = line.Split();
                if (mas[0]==string.Empty) continue;
                if (mas.Length!=2) throw new Exception(".track file is invalide");
                Entries.Add(mas[1],mas[0]=="+"?true:false);
            }
        }
        private List<string> Stringify()
        {
            var L = new List<string>();
            
            foreach(var path in Entries.Keys.OrderBy(path=>path).ToList())
                L.Add($"{(Entries[path]?"+":"-")} {path}");
            return L;
        }
        public void WriteTrack()
        {
            File.WriteAllLines(TrackPath,Stringify());
        }
        public void SetEntry(string path, bool include=false)
        {
            Entries[path]=include;
        }
        public void SetEntries(List<string> paths, bool include=false)
        {
            foreach(var path in paths) SetEntry(path,include);
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
                if (el.Value==true) L.Add(el.Key);
            return L;
        }
        private List<string> _Excluded()
        {
            List<string> L=new List<string>();
            foreach(var el in Entries)
                if (el.Value==false) L.Add(el.Key);
            return L;
        }
    }
}