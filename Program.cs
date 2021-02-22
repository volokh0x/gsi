using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;
using Mono.Unix;
using Mono.Options;

namespace gsi
{
    class Program
    {
        private static string repo {get => Environment.CurrentDirectory;}
        static void Main(string[] args)
        {
          // X.Init(repo);
          X.Add(new string[]{"a"});
          X.Commit("mymsg","myauthor");
          X.LsFiles(true);
          

        }
    }
}
