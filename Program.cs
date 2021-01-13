using System;
using System.IO;
using System.Text;

namespace gsi
{
    class Program
    {
        static string path = Directory.GetCurrentDirectory();
        static string text = "apple";
        static void Main(string[] args)
        {
            X.Init(path);
            var s = X.HashObject(Encoding.UTF8.GetBytes(text), "blob", true);
            Console.WriteLine(s);
        }
    }
}
