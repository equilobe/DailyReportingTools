using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class JiraSprints
    {
        public long maxResults { get; set; }
        public long startAt { get; set; }
        public long total { get; set; }
        public bool isLast { get; set; }
        public List<Sprint> values { get; set; }
    }

    public class Sprint
    {
        public string id { get; set; }
        public string self { get; set; }
        public string state { get; set; }
        public string name { get; set; }
        public string originalBoardId { get; set; }
        public string goal { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string completeDate { get; set; }

        public DateTime? StartDate
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(startDate);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public DateTime? EndDate
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(endDate);
                }
                catch
                {
                    return null;
                }
            }
        }

        public DateTime? CompletedDate
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(completeDate);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
