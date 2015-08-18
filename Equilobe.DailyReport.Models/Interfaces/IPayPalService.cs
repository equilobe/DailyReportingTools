using Equilobe.DailyReport.Models.PayPal;
using Equilobe.DailyReport.Models.Storage;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IPayPalService : IService
    {
        void GetStatus(byte[] parameters, PayPalCheckoutInfo payPalCheckoutInfo, UserManager<ApplicationUser> userManager);
    }
}
