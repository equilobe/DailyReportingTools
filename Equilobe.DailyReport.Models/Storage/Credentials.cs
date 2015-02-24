using Equilobe.DailyReport.Models.Interfaces;

namespace Equilobe.DailyReport.Models.Storage
{
    public class Credentials : ISourceControlRequestContext
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
