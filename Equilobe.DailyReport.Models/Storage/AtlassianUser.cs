using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Storage
{
    public class AtlassianUser
    {
        public long Id { get; set; }
        public long InstalledInstanceId { get; set; }
        public string DisplayName { get; set; }
        public string Key { get; set; }
        public string EmailAddress { get; set; }
        public string AvatarFileName { get; set; }
        public bool IsActive { get; set; }
        public bool IsStalling { get; set; }

        public virtual InstalledInstance InstalledInstance { get; set; }
        public virtual ICollection<AtlassianWorklog> Worklogs { get; set; }
    }
}
