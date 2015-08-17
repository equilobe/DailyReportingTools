using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Errors
{
    public class ApplicationErrors
    {
        public static string UserAlreadyCreated = "User was already created";
        public static string InvalidJiraCredentials = "Invalid JIRA username or password";
        public static string EmailNotAvailable (string email)
        {
            return string.Format("There is already an account using {0} email adress", email);
        }
    }
}
