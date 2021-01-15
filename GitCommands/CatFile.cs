using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    partial class X
    {
        public struct TreeEntry
        {
            public int mode;
            public string path;
            public string sha1;
        }
        public static List<TreeEntry> ReadTree(string sha1_prefix=null, byte[] _data=null)
        {
            ObjectType objt; byte[] data=_data;
            if (sha1_prefix!=null)
            {
                (objt, data)=ReadObject(sha1_prefix);
                if (objt!=ObjectType.tree)
                    throw new Exception();
            }
            else if (_data==null)
                throw new Exception();
            int i=0, end; List<TreeEntry> L=new List<TreeEntry>();
            for (int _=0; _<1000; _++)
            {
                end=Array.IndexOf(data, (byte)0, i);
                if (end==-1)
                    break;
                byte[] header=new byte[end-i];
                Buffer.BlockCopy(data, i, header, 0, header.Length);
                string[] mas = Encoding.UTF8.GetString(header).Split(' ');
                var te = new TreeEntry();
                te.mode=Convert.ToInt32(mas[0], 8);
                te.path=mas[1];
                byte[] hash=new byte[20];
                Buffer.BlockCopy(data, end+1, hash, 0, hash.Length);
                te.sha1=string.Concat(hash.Select(b => b.ToString("x2")));
                L.Add(te);
                i=end+1+20;
            }
            return L;
        }
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
                    foreach(var el in ReadTree(_data:data))
                    { 
                        string type_str=((el.mode>>12)==4)?"tree":"blob";
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
