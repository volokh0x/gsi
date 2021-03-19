using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace gsi
{
    class Config
    {
        private string ConfigPath;
        private Dictionary<(string,string),Dictionary<string,string>> Content = new Dictionary<(string, string), Dictionary<string, string>>();
        public Config(string path, bool read_config=false)
        {
            ConfigPath=path;
            if (read_config) ReadConfig();
        }
        public void ReadConfig()
        {
            // { "remote": {} } ??
            Content = File.ReadAllText(ConfigPath).Split("[")
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
        public void WriteConfig()
        {  
            using(var fd=new StreamWriter(ConfigPath))
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
        public bool Contains(string section, string subsection, string option=null)
        {
            bool b = Content.ContainsKey((section,subsection));
            if (option==null || !b) return b;
            return Content[(section,subsection)].ContainsKey(option); 
        }
        public string GetOptionValue(string section, string subsection, string option)
        {
            if (!Contains(section,subsection,option)) return null;
            return Content[(section,subsection)][option];
        }
        public void SetOptionValue(string section, string subsection, string option, string value)
        {
            if (!Contains(section,subsection))
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