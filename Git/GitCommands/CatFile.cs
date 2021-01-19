using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class X
    {
        
        public static void CatFile(string mode, string sha1_prefix)
        {
            (ObjectType objt, byte[] data)=ReadObject(sha1_prefix);
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
                        string mode_oct=Convert.ToString(el.mode, 8);
                        if (mode_oct.Length==5) mode_oct="0"+mode_oct;
                        Console.WriteLine($"{mode_oct} {type_str} {el.sha1}\t{el.path}");  
                    }
                }
            }
            else
                throw new Exception();
        }
    }
}
