using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DailyReportWeb.Services
{
    public class JiraRequestContextService : IJiraRequestContextService
    {
        public JiraRequestContext Context
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new InvalidOperationException();
            }
        }
    }
}