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
        public CommitContent Content;
        public string Text {get=>string.Join("\n", Stringify());}
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
        public Commit(GitFS gitfs, string hash)
        {
            this.gitfs=gitfs;
            Hash=hash;
            ReadCommit();
        }
        public void ReadCommit()
        {
            (byte[] data, ObjectType objt) = Object.ReadObject(OPath);
            if (objt!=ObjectType.commit) throw new Exception("not a commit");
            string[] lines = Encoding.UTF8.GetString(data).Split("\n");
            
            Content.parent_hashes=new List<string>();
            foreach(var line in lines)
            {
                if (line.StartsWith("tree ")) Content.tree_hash=line.Split(" ")[1];
                else if (line.StartsWith("parent ")) 
                {
                    Content.parent_hashes.Add(line.Split(" ")[1]);
                }
                else if (line.StartsWith("author "))
                {
                    var mas = line.Split(" ");
                    Content.author.name=mas[1];
                    Content.author.email=mas[2];
                    Content.author.timestamp=Convert.ToInt32(mas[3]);
                    Content.author.utc_offset=StructConverter.UTC_OffsetToInt(mas[4]);
                }
                else if (line.StartsWith("comitter "))
                {
                    var mas = line.Split(" ");
                    Content.comitter.name=mas[1];
                    Content.comitter.email=mas[2];
                    Content.comitter.timestamp=Convert.ToInt32(mas[3]); 
                    Content.comitter.utc_offset=StructConverter.UTC_OffsetToInt(mas[4]);
                }
                else if (line!="") 
                    Content.message=line;
            }
        }
        private List<string> Stringify()
        {
            List<string> L = new List<string>();
            L.Add($"tree {Content.tree_hash}");
            foreach(var parent_hash in Content.parent_hashes) L.Add($"parent {parent_hash}");
            L.Add($"author {Content.author.name} {Content.author.email} {Content.author.timestamp} {StructConverter.UTC_OffsetToStr(Content.author.utc_offset)}");
            L.Add($"comitter {Content.comitter.name} {Content.comitter.email} {Content.comitter.timestamp} {StructConverter.UTC_OffsetToStr(Content.comitter.utc_offset)}");
            L.Add($"");
            L.Add($"{Content.message}");
            L.Add($"");
            return L;
        }
        public string HashCommit() 
        {
            var L = Stringify();   
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
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach(var parent_hash in Content.parent_hashes) sb.AppendLine($"🖼 {parent_hash}");
            sb.AppendLine($"🖂 {Content.author.name} {Content.author.email}");
            sb.AppendLine($"🖋 {Content.message}"); 
            sb.AppendLine($"🗀 {Content.tree_hash}");
            return sb.ToString();
        }
    }
}