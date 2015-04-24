using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

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


        [AllowAnonymous]
        public SimpleResult Register(RegisterModel model)
        {
            if (!Validations.Mail(model.Email) ||
                !Validations.Password(model.Password) ||
                !Validations.Url(model.BaseUrl))
                throw new ArgumentException();

            var credentialsValid = JiraService.CredentialsValid(model, false);
            if (!credentialsValid)
                return SimpleResult.Error("Invalid JIRA username or password");

            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email
            };

            IdentityResult result = UserManager.Create(user, model.Password);
            if (!result.Succeeded)
                return SimpleResult.Error(result.Errors.First());

            DataService.SaveInstance(model);

            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            string code = HttpUtility.UrlEncode(UserManager.GenerateEmailConfirmationToken(user.Id));
            var callbackUrl = string.Format("{0}/app/confirmEmail?userId={1}&code={2}",
                                             UrlExtensions.GetHostUrl(Request.RequestUri.OriginalString),
                                             user.Id,
                                             code);

            UserManager.SendEmail(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            return SimpleResult.Success("Account confirmation details has been sent to your mail.");
        }

        [AllowAnonymous]
        [HttpPost]
        public SimpleResult ConfirmEmail([FromBody]EmailConfirmation emailConfirmation)
        {
            string userId = emailConfirmation.userId;
            string code = HttpUtility.UrlDecode(emailConfirmation.code);

            if (userId == null || code == null)
                return SimpleResult.Error("Invalid activation token.");

            var user = UserManager.FindById(userId);
            if (user == null)
                return SimpleResult.Error("Invalid activation token.");
            if (user.EmailConfirmed)
                return SimpleResult.Error("Your account was already activated.");

            IdentityResult result = UserManager.ConfirmEmail(userId, code);
            if (!result.Succeeded)
                return SimpleResult.Error(result.Errors.First());

            var instanceId = UserManager.FindById(userId)
                                        .InstalledInstances
                                        .Single()
                                        .Id;
            SettingsService.SyncAllBasicSettings(new ItemContext(instanceId));

            return SimpleResult.Success("Your account was activated. You can now sign in.");
        }

        [AllowAnonymous]
        public SimpleResult Login(LoginModel model)
        {
            if (!Validations.Mail(model.Email))
                throw new ArgumentException();

            var user = UserManager.Find(model.Email, model.Password);

            if (user == null || UserManager.FindByEmail(model.Email) == null)
                return SimpleResult.Error("Invalid username or password.");

            if (!user.EmailConfirmed)
                return SimpleResult.Error("Account has not been activated yet.");

            SignIn(user, model.RememberMe);

            return SimpleResult.Success("");
        }

        private void SignIn(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, UserManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie));
        }

        public void Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}
