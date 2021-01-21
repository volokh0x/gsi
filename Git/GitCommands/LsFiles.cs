using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace gsi
{
    partial class X
    {
        public static void LsFiles(bool details=false)  
        {
            foreach(var ie in GitPath.ReadIndex())
            {
                if (details)
                {
                    int stage=(ie.flags>>12)&3; 
                    // '{:6o} {} {:}\t{}'
                    string mode_oct=Convert.ToString(ie.mode, 8);
                    if (mode_oct.Length==5) mode_oct="0"+mode_oct;
                    Console.WriteLine($"{mode_oct} {ie.sha1} {stage}\t{ie.path}");
                }
                else {
                    Console.WriteLine(ie.path);
                }
            }
        }
    }
}
