using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace gsi
{
    class Config
    {
        private string FullPath;
        private Dictionary<(string,string),Dictionary<string,string>> Content;
        public Config(string full_path, bool read=false)
        {
            FullPath=full_path;
            if (read) ReadFile();
        }
        public void ReadFile()
        {
            // { "remote": {} } ??
            Content = File.ReadAllText(FullPath).Split("[")
                        .Select(s => s.Trim())
                        .Where(s => s != string.Empty)
                        .Aggregate
                        (
                            new Dictionary<(string,string),Dictionary<string,string>>(), (d,item) => 
                            {
                                var lines = item.Split("\n");
                                string section = new Regex(@"([^ \]]+)( |\])").Match(lines[0]).Groups[1].Value;
                                var match = new Regex("\"(.+)\"").Match(lines[0]);
                                string subsection = match.Success?match.Groups[1].Value:null;
                                if (!d.ContainsKey((section,subsection)))
                                    d[(section,subsection)]=new Dictionary<string, string>();
                                for (int i=1; i<lines.Length; i++)
                                {
                                    string[] mas = lines[i].Split("=").Select(s=>s.Trim()).ToArray();
                                    d[(section,subsection)][mas[0]]=mas[1];
                                }
                                return d;
                            } 
                        );
        }
        public void WriteFile()
        {  
            using(var fd=new StreamWriter(FullPath))
            {
                foreach ((string section, string subsection) in Content.Keys)
                {
                    string subsection_to_write=subsection==null?"":$" \"{subsection}\"";
                    fd.WriteLine($"[{section}{subsection_to_write}]");
                    foreach((string option,string value) in Content[(section,subsection)])
                        fd.WriteLine($"\t{option} = {value}");
                }
            }
        }
        public bool IsBare()
        {
            return GetOptionValue("core",null,"bare")=="true";
        }
        public void AssertNotBare()
        {
            if (IsBare()) throw new Exception("repo is bare");
        }
        public string GetOptionValue(string section, string subsection, string option)
        {
            if (!Content.ContainsKey((section,subsection)))
                return null;
            if (!Content[(section,subsection)].ContainsKey(option))
                return null;
            return Content[(section,subsection)][option];
        }
        public void SetOptionValue(string section, string subsection, string option, string value)
        {
            if (!Content.ContainsKey((section,subsection)))
                Content[(section,subsection)]=new Dictionary<string, string>();
            Content[(section,subsection)][option]=value;
        }
        public void Print()
        {
            foreach ((string section,string subsection) in Content.Keys)
            {
                string subsection_to_write=subsection==null?"":$" \"{subsection}\"";
                Console.WriteLine($"[{section}{subsection_to_write}]");
                foreach((string option,string value) in Content[(section,subsection)])
                        Console.WriteLine($"\t{option} = {value}");  
            }
        }
    }
}