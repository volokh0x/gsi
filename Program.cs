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
            // X.Init(path);
            X.HashObject(Encoding.UTF8.GetBytes(text), "blob", true);
            X.ReadObject("8e95");
            // var s = X.FindObject("8e95");
            // Console.WriteLine(s);
        }
        // static void Main(string[] args)
        // {
        //     var full_data = new byte[]{ 98, 108, 111, 98, 32, 53, 0, 97, 112, 112, 108, 101};
        //     //byte r =0;
        //     int i = Array.IndexOf(full_data, (byte)0);
        //     Console.WriteLine(i);
        // }
    }
}
