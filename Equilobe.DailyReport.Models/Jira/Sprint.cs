using System;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Sprint
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "state")]
        public string State { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "originalBoardId")]
        public string OriginalBoardId { get; set; }

        [DataMember(Name = "goal")]
        public string Goal { get; set; }

        [DataMember(Name = "startDate")]
        public string StartDate { get; set; }

        [DataMember(Name = "endDate")]
        public string EndDate { get; set; }

        [DataMember(Name = "completeDate")]
        public string CompleteDate { get; set; }

        public DateTime? StartedAt
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(StartDate);
                }
                catch
                {
                    return null;
                }
            }
        }

        public DateTime? EndedAt
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(EndDate);
                }
                catch
                {
                    return null;
                }
            }
        }

        public DateTime? CompletedAt
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(CompleteDate);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
