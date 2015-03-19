using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ICredentials
    {
        string Username { get; set; }
        string Password { get; set; }
    }
}
