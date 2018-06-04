using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class JiraSprints
    {
        public long MaxResults { get; set; }
        public long StartAt { get; set; }
        public long Total { get; set; }
        public bool IsLast { get; set; }
        public List<Sprint> Values { get; set; }
    }

    public class Sprint
    {
        public int Id { get; set; }
        public string Self { get; set; }
        public string State { get; set; }
        public string Name { get; set; }
        public string OriginalBoardId { get; set; }
        public string Goal { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CompleteDate { get; set; }

        public DateTime? StartDateDateTime
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(StartDate);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public DateTime? EndDateDateTime
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

        public DateTime? CompletedDateDateTime
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(CompleteDate);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
