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

        [AllowAnonymous]
        public SimpleResult Register(RegisterModel model)
        {
            return RegistrationService.RegisterUser(model);
        }

        //[AllowAnonymous]
        //public SimpleResult CheckRegistrationDetails(RegisterModel model)
        //{
        //    return RegistrationService.CheckRegistrationDetails(model);
        //}

        [AllowAnonymous]
        [HttpPost]
        public SimpleResult ConfirmEmail([FromBody]EmailConfirmation emailConfirmation)
        {
            return RegistrationService.ConfirmEmail(emailConfirmation);
        }

        [AllowAnonymous]
        public SimpleResult Login(LoginModel model)
        {
            return RegistrationService.Login(model);
        }

        public void Logout()
        {
            RegistrationService.Logout();
        }
    }
}
