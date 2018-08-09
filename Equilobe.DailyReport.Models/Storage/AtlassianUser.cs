namespace Equilobe.DailyReport.Models.Storage
{
    public class AtlassianUser
    {
        public long Id { get; set; }
        public long InstalledInstanceId { get; set; }
        public string DisplayName { get; set; }
        public string Key { get; set; }
        public string EmailAddress { get; set; }
        public string Avatar48x48 { get; set; }
        public string Avatar32x32 { get; set; }
        public string Avatar24x24 { get; set; }
        public string Avatar16x16 { get; set; }

        public virtual InstalledInstance InstalledInstance { get; set; }
    }
}
