using System;
using System.IO;
using System.Text;

namespace gsi
{
    partial class X
    {
        
        public static void CatFile(string mode, string prefix)
        {
            ObjectType objt; byte[] data;
            switch(mode){
                case "blob":
                    Console.Write(GitPath.ReadBlob(prefix));
                    break;
                case "tree":
                    foreach(var el in GitPath.ReadTree(prefix))
                    { 
                        string type_str=GitPath.IsDir(el.mode)?"tree":"blob";
                        string mode_oct=GetOctal6(el.mode);
                        Console.WriteLine($"{mode_oct} {type_str} {el.sha1}\t{el.path}");  
                    }
                    break;
                case "commit":
                    Console.Write(GitPath.ReadCommit(prefix));
                    break;
                default:
                    (objt, data)=GitPath.ReadObject(prefix);
                    if (mode=="size") Console.WriteLine(data.Length);
                    if (mode=="type") Console.WriteLine(objt);
                    break;
            }
        }
    }
}
