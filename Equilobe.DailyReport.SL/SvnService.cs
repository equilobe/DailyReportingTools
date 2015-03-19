using Equilobe.DailyReport.BL.Svn;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.SL
{
    public class SvnService : ISvnService
    {
        private SvnClient GetSvnClient(ISourceControlContext context)
        {
            return new SvnClient(context);
        }

        public Log GetLog(ISourceControlContext context, string pathToLog)
        {
            return GetSvnClient(context).GetLog(pathToLog);
        }
    }
}
