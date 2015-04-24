using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Web
{
    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string BaseUrl { get; set; }
        public string TimeZone { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class EmailConfirmation
    {
        public string userId { get; set; }
        public string code { get; set; }
    }
}
