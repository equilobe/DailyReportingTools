using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class Commit
    {
        [DataMember(Name = "hash")]
        public string Hash { get; set; }

        [DataMember(Name = "repository")]
        public Repository Repository { get; set; }

        [DataMember(Name = "links")]
        public Links Links { get; set; }

        [DataMember(Name = "summary")]
        public Summary Summary { get; set; }

        [DataMember(Name = "date")]
        public string Date { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "parents")]
        public List<CommitParent> CommitParrents { get; set; } 

        [DataMember(Name = "author")]
        public CommitAuthor Author { get; set; }

        public DateTime? CreatedAt
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(Date);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
