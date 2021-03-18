// using System;
// using System.IO;

// namespace gsi
// {
//     class Blob : Object
//     {
//         byte[] ObjectContent;
//         string UserFilePath;
//         byte[] UserFileContent;

//         public Blob(string path, bool user_file)
//         {
//             if(user_file)
//             {
//                 UserFilePath=path;
//                 UserFileContent=File.ReadAllBytes(path);
//             }
//             else
//             {
//                 ObjectPath=path;
//                 (byte[] data, ObjectType objt)=Object.ReadObject(path);
//                 if (objt!=ObjectType.blob) throw new Exception("not a blob");
//                 ObjectContent=data;
//             }
//         }
//         public string HashBlob()
//         {
//             (byte[] data, string hash) = Object.HashObject(UserFileContent,ObjectType.blob);
//             ObjectContent=data;
//             ObjectPath=gitp.ObjectFullPath(hash);
//             return hash;
//         }
//         public string WriteBlob()
//         {
//             string hash=HashBlob();
//             Object.WriteObject(ObjectContent,ObjectType.blob,ObjectPath);
//             return hash;
//         }
//     }
// }