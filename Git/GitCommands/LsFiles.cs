using System;

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
                    int stage=GitPath.FlagstStage(ie.flags);
                    string mode_oct=GetOctal6(ie.mode);
                    Console.WriteLine($"{mode_oct} {ie.sha1} {stage}\t{ie.path}");
                }
                else 
                {
                    Console.WriteLine(ie.path);
                }
            }
        }
    }
}
