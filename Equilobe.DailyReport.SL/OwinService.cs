using Equilobe.DailyReport.SL.Interfaces;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Equilobe.DailyReport.SL
{
    public class OwinService : IOwinService
    {
        public ApplicationUserManager GetApplicationUserManager()
        {
            return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public IAuthenticationManager GetAuthenticationManager()
        {
            return HttpContext.Current.GetOwinContext().Authentication;
        }
    }
}
