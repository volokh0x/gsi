﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

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

            X.Status();
        }

        static void MainX(string[] args)
        {
        }
    }
}
