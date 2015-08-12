using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IRegistrationService : IService
    {
        SimpleResult RegisterUser(RegisterModel model, UserManager<ApplicationUser> userManager);

        SimpleResult ConfirmEmail(EmailConfirmation emailConfirmation, UserManager<ApplicationUser> userManager);

        SimpleResult Login(LoginModel model, UserManager<ApplicationUser> userManager);

        void Logout();
    }
}
