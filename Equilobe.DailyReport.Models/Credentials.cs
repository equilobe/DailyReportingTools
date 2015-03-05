using Equilobe.DailyReport.Models.Interfaces;

namespace Equilobe.DailyReport.Models
{
    public class Credentials : ICredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
