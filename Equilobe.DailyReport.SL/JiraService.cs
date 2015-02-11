using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Services;
using Equilobe.JiraConnector;

namespace Equilobe.DailyReport.SL
{
    public class JiraService : IJiraService
    {
        public string GetSprintName(IJiraRequestContext context)
        {
            return GetClient(context).GetSprintName();
        }

        private JiraClient GetClient(IJiraRequestContext context)
        {
            if (context.SharedSecret != null)
                return new JiraClient(context.BaseUrl, context.SharedSecret);
            else
                return new JiraClient(context.BaseUrl, context.Username, context.Password);
        }
    }
}
