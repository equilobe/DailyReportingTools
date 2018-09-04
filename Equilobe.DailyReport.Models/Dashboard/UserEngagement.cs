using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class UserEngagement
    {
        public long CommentsCount { get; set; }
        public long CommitsCount { get; set; }
        public long LinesOfCodeAdded { get; set; }
        public long LinesOfCodeDeleted { get; set; }
    }
}
