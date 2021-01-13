using System;
using System.IO;
using System.Text;

namespace gsi
{
    class Program
    {
        private static string repo {get => Environment.CurrentDirectory;}
        static string text = "apple";
        static void Main(string[] args)
        {
            X.Init(Environment.CurrentDirectory);
            X.HashObject(Encoding.UTF8.GetBytes(text), ObjectType.blob, true);
            X.ReadObject("8e95");
            var s = X.FindObject("8e95");
            Console.WriteLine(s);
            (ObjectType type, byte[] data) = X.ReadObject("8e95");
            Console.WriteLine($"{type} {Encoding.UTF8.GetString(data)}");
        }
        // static void Main(string[] args)
        // {
        //     string s = ObjectType.blob.ToString();
        //     Console.WriteLine($"<{s}>");
        //     var e = Enum.Parse(typeof(ObjectType), "blob");
        //     Console.WriteLine($"{e.GetType()}");
        // }
    }
}
