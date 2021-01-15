using System;
using System.IO;
using System.Text;

namespace gsi
{
    class Program
    {
        private static string repo {get => Environment.CurrentDirectory;}
        static void Main(string[] args)
        {
            int x = 100644;
            Console.WriteLine(string.Format("{0:D6}", x));
        }
        static void MainX(string[] args)
        {
        }
    }
}
