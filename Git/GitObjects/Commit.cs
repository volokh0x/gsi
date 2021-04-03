using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace gsi
{
    struct ContactInfo
    {
        public string name;
        public string email;
        public int timestamp;
        public int utc_offset;
    }
    struct CommitContent
    {
        public string tree_hash;
        public List<string> parent_hashes;
        public ContactInfo author;
        public ContactInfo comitter;
        public string message;
    }
    class Commit : Object
    {
        public static string AddCommit(GitFS gitfs, string tree_hash, string message)
        {
            return new Commit(
                gitfs, 
                tree_hash, 
                message
            ).WriteCommit();
        }

        public CommitContent Content;
        private byte[] HashedContent;
        
        public Commit(GitFS gitfs, string tree_hash, string message)
        {
            this.gitfs=gitfs;
            var contact_info = new ContactInfo
            {
                name=gitfs.config_set.GetOptionValue("user",null,"name"),
                email=gitfs.config_set.GetOptionValue("user",null,"email"),
                timestamp=StructConverter.TimeStamp(DateTime.UtcNow),
                utc_offset=StructConverter.UTC_Offset()
            };
            Content=new CommitContent
            {
                tree_hash=tree_hash,
                parent_hashes=gitfs.GetParentCommits(),
                author=contact_info,
                comitter=contact_info,
                message=message
            };
        }
        public void ReadCommit()
        {
            (byte[] data, ObjectType objt) = Object.ReadObject(OPath);
            if (objt!=ObjectType.commit) throw new Exception("not a commit");
            // data to Content !!!
        }
        public string HashCommit() 
        {
            List<string> L = new List<string>();
            L.Add($"tree {Content.tree_hash}");
            foreach(var parent_hash in Content.parent_hashes) L.Add($"parent {parent_hash}");
            L.Add($"author {Content.author.name} {Content.author.email} {Content.author.timestamp} {StructConverter.UTC_OffsetToStr(Content.author.utc_offset)}");
            L.Add($"comitter {Content.comitter.name} {Content.comitter.email} {Content.comitter.timestamp} {StructConverter.UTC_OffsetToStr(Content.comitter.utc_offset)}");
            L.Add($"");
            L.Add($"{Content.message}");
            L.Add($"");
            (byte[] hashed_data, string hash)=Object.HashObject(Encoding.UTF8.GetBytes(string.Join("\n", L)), ObjectType.commit);
            HashedContent=hashed_data; Hash=hash;
            return hash;
        }
        public string WriteCommit(bool to_hash=true)
        {
            if (to_hash || HashedContent==null)
                HashCommit();
            Object.WriteObject(HashedContent, ObjectType.commit, OPath); 
            return Hash;
        }
    }
}