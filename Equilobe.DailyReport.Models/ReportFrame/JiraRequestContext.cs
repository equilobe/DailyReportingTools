using Equilobe.DailyReport.Models.Interfaces;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class JiraRequestContext : IJiraRequestContext
    {
        public string BaseUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string SharedSecret { get; set; }

        public JiraRequestContext()
        {

        }

        public JiraRequestContext(string baseUrl, string sharedSecret)
        {
            BaseUrl = baseUrl;
            SharedSecret = sharedSecret;
        }

        public JiraRequestContext(string baseUrl, string username, string password)
        {
            BaseUrl = baseUrl;
            Username = username;
            Password = password;
        }
    }
}
