using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Mono.Unix;

namespace gsi
{
    partial class X
    {
        public static void Add(string[] paths)
        {
            GitPath.AssertValidRoot();
            List<string> file_paths=new List<string>();
            foreach(var path in paths)
                if (File.Exists(path))
                    file_paths.Add(GitPath.FileRelPath(path));
                else if (Directory.Exists(path))
                    file_paths.AddRange(Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories));
                else 
                    throw new Exception($"{path} was not found");
            if (file_paths.Count==0)
                throw new Exception("nothing specified, nothing added");
            AddFiles(file_paths.ToArray());
        }
        private static void AddFiles(string[] paths)  
        {
            List<IndexEnry> L = GitPath.ReadIndex();
            L.RemoveAll(ie=>paths.Contains(ie.path));
            foreach(var path in paths)
            {
                string sha1=X.WriteObject(File.ReadAllBytes(path), ObjectType.blob);
                ushort len=(UInt16)Encoding.UTF8.GetBytes(path).Length;
                ushort flags=(ushort)(len&0b0000_111111111111);

                UnixFileInfo unixFileInfo = new UnixFileInfo(path);
                uint tt = GetTimeStamp(new FileInfo(path).LastWriteTimeUtc);
                uint mode = ((uint)unixFileInfo.Protection)|((uint)unixFileInfo.FileAccessPermissions);

                L.Add(new IndexEnry{ctime_n=tt,
                                    ctime_s=0,
                                    mtime_n=tt,
                                    mtime_s=0,
                                    dev=(uint)unixFileInfo.DeviceType,
                                    ino=(uint)unixFileInfo.Inode,
                                    mode=mode,
                                    uid=(uint)unixFileInfo.OwnerUserId,
                                    gid=(uint)unixFileInfo.OwnerGroupId,
                                    size=(uint)unixFileInfo.Length,
                                    sha1=sha1,
                                    flags=flags,
                                    path=path});
            }
            GitPath.WriteIndex(L.OrderBy(X => X.path).ToList());
        }
    }
}
