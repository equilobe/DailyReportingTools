using Equilobe.DailyReport.BL.Svn;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using System.Collections.Generic;

namespace Equilobe.DailyReport.SL
{
    public class SvnService : ISvnService
    {
        private SvnClient GetSvnClient(ISourceControlContext context)
        {
            return new SvnClient(context);
        }

        public Log GetLog(ISourceControlContext context)
        {
            return GetSvnClient(context).GetLog();
        }

        public Log GetLogWithCommitLinks(ISourceControlContext context)
        {
            return GetSvnClient(context).GetLogWithCommitLinks();
        }

        public List<string> GetAllAuthors(ISourceControlContext context)
        {
            return GetSvnClient(context).GetAllAuthors();
        }
    }
}
