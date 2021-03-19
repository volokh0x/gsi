using System;
using System.IO;
using System.Text;

namespace gsi
{
    class Blob : Object
    {
        public string Content;
        public byte[] HashedContent;
        
        public Blob(string path, bool user_file)
        {
            if (!user_file)
                ObjectPath=path;
            else 
                Content=File.ReadAllText(path);
        }
        public void ReadBlob()
        {
            (byte[] data, ObjectType objt) = Object.ReadObject(ObjectPath);
            if (objt!=ObjectType.blob) throw new Exception("not a blob");
            Content=Encoding.UTF8.GetString(data);
        }
        public string HashBlob()
        {
            (byte[] hashed_data, string hash) = Object.HashObject(Encoding.UTF8.GetBytes(Content),ObjectType.blob);
            HashedContent=hashed_data;
            return hash;
        }
        public void WriteBlob(string path)
        {
            ObjectPath=path;
            Object.WriteObject(HashedContent, ObjectType.blob, ObjectPath); 
        }
    }
}