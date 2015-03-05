using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ISourceControlContext
    {
        SourceControlOptions SourceControlOptions { get; set; }
        DateTime FromDate { get; set; }
        DateTime ToDate { get; set; }
        
    }
}
