using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class ReportSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string BaseUrl { get; set; }
        public long ProjectId { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string SharedSecret { get; set; }
        public string ReportTime { get; set; }
        public string UniqueProjectKey { get; set; }
        public string RootPath { get; set; }

        public string PolicyXml { get; set; }

        // NO DB
        //public JiraPolicy Policy { get; set; }
    }
}
