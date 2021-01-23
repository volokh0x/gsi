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
            // Console.WriteLine(X.HashObject(Encoding.UTF8.GetBytes("apple\n"), ObjectType.blob, true));
            // X.CatFile("pretty", "4277"); 
            // X.LsFiles(true);
            //X.Status();

            PrintIndex();
            X.Add(new string[]{"a.txt"});
            PrintIndex();
        }
        static void PrintIndex()
        {
            foreach (var el in GitPath.ReadIndex())
                Console.WriteLine($"{el.path} {el.mode} {el.sha1} {Convert.ToString(Convert.ToUInt16(Encoding.UTF8.GetBytes(el.path).Length), 2)}");
            Console.WriteLine();
        }
    }
}
