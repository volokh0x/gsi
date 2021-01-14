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
            int x= 12;
            Console.WriteLine(string.Format("{0:D3}", x));
        }
        static void MainX(string[] args)
        {
        }
    }
}
