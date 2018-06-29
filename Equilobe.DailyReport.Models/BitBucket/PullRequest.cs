using Equilobe.DailyReport.Models.Enums;
using System;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class PullRequest
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "state")]
        private string StateType { get; set; }

        [DataMember(Name = "created_on")]
        private string CreatedOn { get; set; }

        [DataMember(Name = "updated_on")]
        private string UpdatedOn { get; set; }

        [DataMember(Name = "comment_count")]
        public int ComentCount { get; set; }

        [DataMember(Name = "task_count")]
        public int TaskCount { get; set; }

        [DataMember(Name = "close_source_branch")]
        public bool CloseSourceBranch { get; set; }

        [DataMember(Name = "reason")]
        public string Reason { get; set; }

        [DataMember(Name = "links")]
        public Links Links { get; set; }

        [DataMember(Name = "summary")]
        public Summary Summary { get; set; }

        [DataMember(Name = "destination")]
        public SourceOrDestination Destination { get; set; }

        [DataMember(Name = "source")]
        public SourceOrDestination Source { get; set; }

        [DataMember(Name = "author")]
        public Author Author { get; set; }

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

        public State State
        {
            get
            {
                return StateType.ToEnum<State>(true);
            }
        }
    }
}
