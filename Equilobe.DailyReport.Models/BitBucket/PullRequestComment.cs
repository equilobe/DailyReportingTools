using System;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class PullRequestComment
    {
        [DataMember(Name = "links")]
        public Links Links { get; set; }

        [DataMember(Name = "deleted")]
        public bool Deleted { get; set; }

        [DataMember(Name = "pullrequest")]
        public PullRequest PullRequest { get; set; }

        [DataMember(Name = "content")]
        public Summary Content { get; set; }

        [DataMember(Name = "created_on")]
        public string CreatedOn { get; set; }

        [DataMember(Name = "updated_on")]
        public string UpdatedOn { get; set; }

        [DataMember(Name = "user")]
        public Author User { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        public DateTime? CreatedAt
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(CreatedOn);
                }
                catch
                {
                    return null;
                }
            }
        }

        public DateTime? UpdatedAt
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(UpdatedOn);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
