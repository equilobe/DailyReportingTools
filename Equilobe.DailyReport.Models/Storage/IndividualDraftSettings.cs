using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class IndividualDraftSettings
    {
        public long Id { get; set; }
        public long GeneratedInfoId { get; set; }
        public string Username { get; set; }
        public string UniqueUserKey { get; set; }
        public DateTime ConfirmationDate { get; set; }
        public DateTime ReportDate { get; set; }
        public bool IsProjectLead { get; set; }
    }
}
