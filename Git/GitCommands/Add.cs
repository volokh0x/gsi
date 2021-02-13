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
            List<IndexEnry> L = GitPath.ReadIndex().Where(ie=>!paths.Contains(ie.path)).ToList();
            foreach(var path in paths)
            {
                string sha1=X.WriteObject(File.ReadAllBytes(path), ObjectType.blob);
                ushort flags=(UInt16)Encoding.UTF8.GetBytes(path).Length;
                
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
