using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<InstalledInstance> InstalledInstances { get; set; }
    }
}
