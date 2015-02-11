using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public interface IJiraRequestContext 
    {
        string BaseUrl { get; }
        string Username { get;  }
        string Password { get; }
        string SharedSecret { get; }
    }
}
