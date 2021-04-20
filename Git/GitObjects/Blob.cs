using System;
using System.IO;
using System.Text;

namespace gsi
{
    class Blob : Object
    {
        public string Content;
        private byte[] HashedContent;
        
        public Blob(GitFS gitfs, string path, bool user_file)
        {
            this.gitfs=gitfs;
            if (!user_file)
            {
                Hash = gitfs.gitp.HashFromPath(path);
                ReadBlob();
            } 
            else 
                Content = File.ReadAllText(path);
        }
        public void ReadBlob()
        {
            (byte[] data, ObjectType objt) = Object.ReadObject(OPath);
            if (objt!=ObjectType.blob) throw new Exception("not a blob");
            Content=Encoding.UTF8.GetString(data);
        }
        public string HashBlob()
        {
            byte[] data = Encoding.UTF8.GetBytes(Content);
            (byte[] hashed_data, string hash) = Object.HashObject(data,ObjectType.blob);
            HashedContent=hashed_data;
            Hash=hash;
            return hash;
        }
        public string WriteBlob(bool to_hash=true)
        {
            if (to_hash || HashedContent==null)
                HashBlob();
            Object.WriteObject(HashedContent, ObjectType.blob, OPath); 
            return Hash;
        }
    }
}