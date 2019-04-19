using Equilobe.DailyReport.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Equilobe.DailyReport.Models.Storage
{
    public class InstalledInstance : IInstance
    {
        public long Id { get; set; }
        [Required]
        public string BaseUrl { get; set; }
        public string UniqueKey { get; set; }
        public string Hash { get; set; }
        public string TimeZone { get; set; }
        public string UserId { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }
        public string ClientKey { get; set; }
        public string SharedSecret { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime? LastSync { get; set; }
    

        public virtual ICollection<BasicSettings> BasicSettings { get; set; }
        public virtual ICollection<UserImage> UserImages { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
        public virtual ICollection<AtlassianUser> AtlassianUsers { get; set; }


        public virtual ApplicationUser User { get; set; }
    }
}
