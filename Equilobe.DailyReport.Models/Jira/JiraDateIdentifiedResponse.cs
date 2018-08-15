using System;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraDateIdentifiedResponse : JiraIdentifiableResponse
    {
        [DataMember(Name = "created")]
        public string Created { get; set; }
        
        [DataMember(Name = "updated")]
        public string Updated { get; set; }

        [DataMember(Name = "startDate")]
        public string StartDate { get; set; }

        public DateTime CreatedAt
        {
            get
            {
                return Convert.ToDateTime(Created);
            }
            set
            {

            }
        }

        public DateTime StartedAt
        {
            get
            {
                return Convert.ToDateTime(StartDate);
            }
            set
            {

            }
        }

        public DateTime UpdatedAt
        {
            get
            {
                return Convert.ToDateTime(Updated);
            }
            set
            {

            }
        }
    }
}
