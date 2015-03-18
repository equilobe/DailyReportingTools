using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IJiraRequestContextService
    {
        JiraRequestContext Context { get; }
    }
}
