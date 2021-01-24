using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;
using Mono.Unix;

namespace gsi
{
    class Program
    {
        private static string repo {get => Environment.CurrentDirectory;}
        static void Main(string[] args)
        {
            // X.Init(repo);
            // X.Add(new string[]{"a.txt"});
            // X.LsFiles(true);
            // X.Status();
            // X.Commit("HI!", "vit <catchme@gmail.com>");
            // X.Status();
            X.Status();
        }
        static void MainX()
        {
            int x=-1;
            uint x1=0;
            uint x2 = (uint)x;
            Console.WriteLine($"{x1==x2} {x1} {x2}");
            Console.WriteLine(uint.MaxValue);
        }
        
        static void PrintIndex()
        {
            foreach (var el in GitPath.ReadIndex())
                Console.WriteLine($"{el.path} {el.mode} {el.sha1} {Convert.ToString(Convert.ToUInt16(Encoding.UTF8.GetBytes(el.path).Length), 2)}");
            Console.WriteLine();
        }
    }
}
