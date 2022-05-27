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
    class Tmp
    {
        private GitFS gitfs;
        public string TmpPath { get => gitfs.gitp.PathFromRootUser(".tmp"); }
        public Dictionary<string, string> Entries = new Dictionary<string, string>();

        public Tmp(GitFS gitfs, bool read_tmp = true)
        {
            this.gitfs = gitfs;
            if (read_tmp) ReadTmp();
        }
        public Tmp(GitFS gitfs, string hash)
        {
            this.gitfs = gitfs;
            var blob = new Blob(gitfs, gitfs.gitp.PathFromHash(hash), false);
            ReadTmp(blob.Content.Split(Environment.NewLine));
        }
        public void ReadTmp()
        {
            ReadTmp(File.ReadAllLines(TmpPath));
        }
        public void ReadTmp(string[] lines)
        {
            foreach (var line in lines)
            {
                var mas = line.Split();
                if (mas[0] == string.Empty) continue;
                if (mas.Length != 2) throw new Exception(".tmp file is invalide");
                string path = mas[0], hash = mas[1];
                Entries.Add(path, hash);
            }
        }
        private List<string> Stringify()
        {
            var L = new List<string>();
            foreach (var path in Entries.Keys.OrderBy(path => path).ToList())
                L.Add($"{path} {Entries[path]}");
            return L;
        }
        public void WriteTmp()
        {
            File.WriteAllLines(TmpPath, Stringify());
        }
        public string WriteLikeBlob()
        {
            string str_content = string.Join(System.Environment.NewLine, Stringify());
            if (str_content != string.Empty)
                str_content += System.Environment.NewLine;
            var blob = new Blob(gitfs, str_content);
            return blob.WriteBlob();
        }
        public void SetEntriesFromTrack()
        {
            bool write_track = false;
            Entries = new Dictionary<string, string>();
            List<string> tmps = gitfs.track.tmp;
            foreach (var path in tmps)
            {
                if (!File.Exists(path))
                {
                    gitfs.track.RemoveEntry(path);
                    write_track = true;
                    continue;
                }
                string hash = new Blob(gitfs, path, true).WriteBlob();
                Entries.Add(path, hash);
            }
            if (write_track)
                gitfs.track.WriteTrack();
        }
        public void SetEntry(string path, string hash)
        {
            Entries[path] = hash;
        }
        public void RemoveEntry(string path)
        {
            string blob_path = gitfs.gitp.PathFromHash(Entries[path]);
            Entries.Remove(path);
            if (!File.Exists(blob_path)) return;
            File.Delete(blob_path);
            string ab_path = blob_path.Substring(0, blob_path.Length - 38);
            if (Directory.GetFiles(ab_path).Length == 0)
                Directory.Delete(ab_path);
        }
        public void RemoveEntries(List<string> paths)
        {
            if (paths == null)
                paths = Entries.Keys.ToList();
            foreach (var path in paths)
                RemoveEntry(path);
        }
        public bool IsFilePresent(string path)
        {
            return Entries.Keys.Contains(path);
        }
    }
}