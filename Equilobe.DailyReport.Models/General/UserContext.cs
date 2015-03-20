using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Equilobe.DailyReport.Models.General
{
    public class UserContext
    {
        public UserContext()
        {
            if (Thread.CurrentPrincipal != null)
                UserId = Thread.CurrentPrincipal.Identity.GetUserId();
        }

        public string UserId { get; set; }
    }
}
