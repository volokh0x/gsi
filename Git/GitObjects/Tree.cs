// using System;
// using System.IO;
// using System.Collections.Generic;
// using Mono.Unix;

// namespace gsi
// {
//     struct TreeEntry
//     {
//         public int mode;
//         public string path;
//         public string hash;
//     }
//     class Tree : Object
//     {
//         string UserDirPath; 
//         List<TreeEntry> ObjectContent;
//         public Tree(string path, List<IndexEnry> ies)
//         {
//             UserDirPath=path;
//             ObjectContent=new List<TreeEntry>();
//             // implement index first
//         }
//         public Tree(string path)
//         {
//             (byte[] data, ObjectType objt) = Object.ReadObject(path);
//             if (objt!=ObjectType.tree) throw new Exception("not a tree");
//             // data to ObjectContent
//         }
//         public (byte[],string) HashTree()
//         {

//         }
//         public string WriteTree()
//         {

//         }
//     }
// }