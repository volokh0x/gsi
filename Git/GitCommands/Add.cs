using System;
using System.IO;
using System.IO.Compression;
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
            List<IndexEnry> L = GitPath.ReadIndex().Where(X=>!paths.Contains(X.path)).ToList();
            foreach(var path in paths)
            {
                string sha1=X.HashObject(File.ReadAllBytes(path), ObjectType.blob);
                ushort flags=Convert.ToUInt16(Encoding.UTF8.GetBytes(path).Length);
                // ???
                // if (flags < (1 << 12))
                //     throw new Exception(">>flag excpetion");

                UnixFileInfo unixFileInfo = new UnixFileInfo(path);
                FileInfo info = new FileInfo(path);
                var diff = info.LastWriteTimeUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                uint tt=Convert.ToUInt32(Math.Floor(diff.TotalSeconds));

                uint mode = ((uint)unixFileInfo.Protection)|((uint)unixFileInfo.FileAccessPermissions);
                L.Add(new IndexEnry{ctime_n=tt,
                                    ctime_s=0,
                                    mtime_n=tt,
                                    mtime_s=0,
                                    dev=Convert.ToUInt32(unixFileInfo.DeviceType),
                                    ino=Convert.ToUInt32(unixFileInfo.Inode),
                                    mode=mode,
                                    uid=Convert.ToUInt32(unixFileInfo.OwnerUserId),
                                    gid=Convert.ToUInt32(unixFileInfo.OwnerGroupId),
                                    size=Convert.ToUInt32(unixFileInfo.Length),
                                    sha1=sha1,
                                    flags=flags,
                                    path=path});
            }
            GitPath.WriteIndex(L.OrderBy(X => X.path).ToList());
        }
    }
}
