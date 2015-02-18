using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public interface IRequestContext
    {
        string Username { get; set; }
        string Password { get; set; }
    }
}
