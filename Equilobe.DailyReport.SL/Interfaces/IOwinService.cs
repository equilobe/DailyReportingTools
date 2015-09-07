using Equilobe.DailyReport.Models.Interfaces;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.SL.Interfaces
{
    public interface IOwinService : IService
    {
        ApplicationUserManager GetApplicationUserManager();

        IAuthenticationManager GetAuthenticationManager();
    }
}
