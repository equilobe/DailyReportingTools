using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Jira;

namespace Equilobe.DailyReport.Models.Services
{
    public interface IJiraService
    {
        string GetSprintName(IJiraRequestContext context);
    }
}
