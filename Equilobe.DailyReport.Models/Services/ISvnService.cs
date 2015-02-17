using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Services
{
    public interface ISvnService
    {
        Log GetLog(ISourceControlContext context, string pathToLog);
    }
}
