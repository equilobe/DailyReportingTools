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
        public async Task<AccountResponse> Register(RegisterModel model)
        {
            if (!Validations.Mail(model.Email) ||
                !Validations.Password(model.Password) ||
                !Validations.Url(model.BaseUrl))
                throw new ArgumentException();

            var credentialsValid = JiraService.CredentialsValid(model, false);
            if (!credentialsValid)
                return new AccountResponse()
                {
                    Success = false,
                    Message = "Invalid JIRA username or password"
                };

            List<string> errors = new List<string>();

            if (errors.Count != 0)
            {
                return new AccountResponse()
                {
                    Success = false,
                    ErrorList = errors,
                    Message = "Correct errors first."
                };
            }

            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email
            };
            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return new AccountResponse()
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault(),
                    ErrorList = result.Errors.ToList()
                };

            DataService.SaveInstance(model);

            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            string code = HttpUtility.UrlEncode(await UserManager.GenerateEmailConfirmationTokenAsync(user.Id));
            var callbackUrl = string.Format("{0}/app/confirmEmail?userId={1}&code={2}",
                                             UrlExtensions.GetHostUrl(Request.RequestUri.OriginalString),
                                             user.Id,
                                             code);

            await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            return new AccountResponse()
            {
                Success = true,
                Message = "Account confirmation details has been sent to your mail."
            };
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<AccountResponse> ConfirmEmail([FromBody]EmailConfirmation emailConfirmation)
        {
            string userId = emailConfirmation.userId;
            string code = HttpUtility.UrlDecode(emailConfirmation.code);
            if (userId == null || code == null)
                return new AccountResponse() { Success = false };

            if (UserManager.FindById(userId).EmailConfirmed)
                return new AccountResponse()
                {
                    Success = false,
                    Message = "Your account was already activated."
                };

            IdentityResult result = await UserManager.ConfirmEmailAsync(userId, code);
            if (!result.Succeeded)
                return new AccountResponse()
                {
                    Success = false,
                    Message = result.Errors.First()
                };

            var instanceId = UserManager.FindById(userId)
                                        .InstalledInstances
                                        .Single()
                                        .Id;
            SettingsService.SyncAllBasicSettings(new ItemContext(instanceId));

            return new AccountResponse()
            {
                Success = true,
                Message = "Your account was activated. You can now sign in."
            };
        }

        [AllowAnonymous]
        public async Task<AccountResponse> Login(LoginModel model)
        {
            if (!Validations.Mail(model.Email))
                throw new ArgumentException();

            List<string> errors = new List<string>();
            if (errors.Count != 0)
                return new AccountResponse()
                {
                    Success = false,
                    ErrorList = errors,
                    Message = "Correct errors first."
                };

            var user = await UserManager.FindAsync(model.Email, model.Password);

            if (user == null || UserManager.FindByEmail(model.Email) == null)
                return new AccountResponse()
                {
                    Success = false,
                    ErrorList = errors,
                    Message = "Invalid username or password."
                };

            if (!UserManager.IsEmailConfirmed(user.Id))
                return new AccountResponse()
                {
                    Success = false,
                    Message = "Account has not been activated yet."
                };

            await SignInAsync(user, model.RememberMe);
            return new AccountResponse()
            {
                Success = true
            };
        }

        public AccountResponse Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return new AccountResponse()
            {
                Success = true
            };
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
        }

    }
}
