using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Views;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class AccountController : ApiController
    {
        public IDataService DataService { get; set; }
        public ISettingsService SettingsService { get; set; }
        public IJiraService JiraService { get; set; }
        public IRegistrationService RegistrationService { get; set; }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //[AllowAnonymous]
        //public SimpleResult Register(RegisterModel model)
        //{
        //    return RegistrationService.RegisterUser(model, UserManager);           
        //}

        [AllowAnonymous]
        public SimpleResult CheckRegistrationDetails(RegisterModel model)
        {
            return RegistrationService.CheckRegistrationDetails(model, UserManager);
        }

        [AllowAnonymous]
        [HttpPost]
        public SimpleResult ConfirmEmail([FromBody]EmailConfirmation emailConfirmation)
        {
            return RegistrationService.ConfirmEmail(emailConfirmation, UserManager);
        }

        [AllowAnonymous]
        public SimpleResult Login(LoginModel model)
        {
            return RegistrationService.Login(model, UserManager);
        }

        public void Logout()
        {
            RegistrationService.Logout();
        }
    }
}
