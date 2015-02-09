using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Enums
{
    public enum SourceControlType { GitHub, SVN };
    public enum ErrorType { HasRemaining, HasNoTimeSpent, HasNoRemaining, Unassigned, NotConfirmed };
    public enum Health { Bad, Weak, Good, None };
}
