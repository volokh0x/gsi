using System;
using System.IO;
using System.Text;

namespace gsi
{
    partial class X
    {
        
        public static void CatFile(string mode, string prefix)
        {
            (ObjectType objt, byte[] data)=GitPath.ReadObject(prefix);
            if (mode=="commit" || mode=="tree" || mode=="blob")
            {
                if (objt.ToString()!=mode)
                    throw new Exception();
                Console.Write(Encoding.UTF8.GetString(data));
            }
            else if (mode=="size")
                Console.WriteLine(data.Length);
            else if (mode=="type")
                Console.WriteLine(objt);
            else if (mode=="pretty")
            {
                if (objt==ObjectType.blob || objt==ObjectType.commit)
                    Console.Write(Encoding.UTF8.GetString(data));
                else if (objt==ObjectType.tree)
                {
                    foreach(var el in GitPath.ReadTree(_data:data))
                    { 
                        string type_str=GitPath.IsDir(el.mode)?"tree":"blob";
                        string mode_oct=GetOctal6(el.mode);
                        Console.WriteLine($"{mode_oct} {type_str} {el.sha1}\t{el.path}");  
                    }
                }
            }
            else
                throw new Exception();
        }
    }
}
