using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Equilobe.DailyReport.Models.Storage
{
    public class InstalledInstance
    {
        public long Id { get; set; }
        [Required]
        public string BaseUrl { get; set; }
        public string TimeZone { get; set; }
        public string SharedSecret { get; set; }
        public string UserId { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }
		

        public virtual ICollection<ReportSettings> ReportSettings { get; set; }

		public virtual ApplicationUser User { get; set; }
    }
}
