using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Services
{
    class JiraRequestContextService : IJiraRequestContextService
    {
        JiraRequestContext _context = null;

        public JiraRequestContext Context
        {
            get
            {
                if (_context == null)
                    throw new InvalidOperationException("Call set first!");

                return _context;
            }
            set
            {
                if (_context != null)
                    throw new InvalidOperationException("You can only call set once!");

                if (value == null)
                    throw new ArgumentNullException();

                _context = value;
                    
            }
        }
    }
}
