using System;
using System.Collections.Generic;

namespace gsi
{
    partial class X
    {
        public static void Status()  
        {
            (List<string> lch, List<string> lnew, List<string> ldel)=GitPath.GetStatus();
            if (lch.Count>0)
            {
                Console.WriteLine("changed files:");
                foreach(var el in lch) Console.WriteLine("\t"+el);
                Console.WriteLine();
            }
            if (lnew.Count>0)
            {
                Console.WriteLine("new files:");
                foreach(var el in lnew) Console.WriteLine("\t"+el);
                Console.WriteLine();
            }
            if (ldel.Count>0)
            {
                Console.WriteLine("deleted files:");
                foreach(var el in ldel) Console.WriteLine("\t"+el);
                Console.WriteLine();
            }
        }
    }
}
