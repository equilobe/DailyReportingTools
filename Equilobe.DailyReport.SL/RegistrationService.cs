using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Mail;
using System.IO;
using Equilobe.DailyReport.Models.Views;
using System.Net.Http;
using Equilobe.DailyReport.Models.Data;
using Equilobe.DailyReport.SL.Interfaces;

namespace Equilobe.DailyReport.SL
{
    public class RegistrationService : IRegistrationService
    {
        public IJiraService JiraService { get; set; }
        public IDataService DataService { get; set; }
        public ISettingsService SettingsService { get; set; }
        public IConfigurationService ConfigurationService { get; set; }
        public IEmailService EmailService { get; set; }
        public IOwinService OwinService { get; set; }

        public SimpleResult RegisterUser(RegisterModel model)
        {
            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email
            };

            var userManager = OwinService.GetApplicationUserManager();

            var searchedUser = userManager.FindByEmail(user.Email);
            if (searchedUser != null)
                return SimpleResult.Error(ApplicationErrors.EmailNotAvailable(searchedUser.Email));

            var credentialsValid = JiraService.CredentialsValid(model, false);
            if (!credentialsValid)
                return SimpleResult.Error(ApplicationErrors.InvalidJiraCredentials);

            IdentityResult result = userManager.Create(user, model.Password);

            if (!result.Succeeded)
                return SimpleResult.Error(result.Errors.First());

            var callbackUrl = GetCallbackUrl(user.Id);
            SendAccountConfirmationEmail(user, callbackUrl);

            DataService.SaveInstance(model);

            return SimpleResult.Success("Account confirmation details has been sent to your mail.");
        }

        public SimpleResult ConfirmEmail(EmailConfirmation emailConfirmation)
        {
            string userId = emailConfirmation.userId;
            string code = HttpUtility.UrlDecode(emailConfirmation.code);

            if (userId == null || code == null)
                return SimpleResult.Error("Invalid activation token.");

            var userManager = OwinService.GetApplicationUserManager();

            var user = userManager.FindById(userId);
            if (user == null)
                return SimpleResult.Error("Invalid activation token.");
            if (user.EmailConfirmed)
                return SimpleResult.Error("Your account was already activated.");

            IdentityResult result = userManager.ConfirmEmail(userId, code);
            if (!result.Succeeded)
                return SimpleResult.Error(result.Errors.First());

            var instanceId = userManager.FindById(userId)
                                        .InstalledInstances
                                        .Single()
                                        .Id;
            SettingsService.SyncAllBasicSettings(new ItemContext(instanceId));

            return SimpleResult.Success("Your account was activated. You can now sign in.");
        }

        public SimpleResult Login(LoginModel model)
        {
            ValidateMail(model);
            var userManager = OwinService.GetApplicationUserManager();

            var user = userManager.Find(model.Email, model.Password);

            if (user == null || userManager.FindByEmail(model.Email) == null)
                return SimpleResult.Error("Invalid username or password.");

            if (!user.EmailConfirmed)
                return SimpleResult.Error("Account has not been activated yet.");

            SignIn(user, model.RememberMe);

            return SimpleResult.Success("");
        }

        public void Logout()
        {
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;

            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        #region Helpers

        private string GetCallbackUrl(string userId)
        {
            string code = HttpUtility.UrlEncode(OwinService.GetApplicationUserManager().GenerateEmailConfirmationToken(userId));

            return string.Format("{0}/app/confirmEmail?userId={1}&code={2}", ConfigurationService.GetWebBaseUrl(), userId, code);
        }

        private static void ValidateRegisterModel(RegisterModel model)
        {
            if (!Validations.Mail(model.Email) ||
                !Validations.Password(model.Password) ||
                !Validations.Url(model.BaseUrl))
                throw new ArgumentException();
        }

        private static void ValidateMail(LoginModel model)
        {
            if (!Validations.Mail(model.Email))
                throw new ArgumentException();
        }

        private void SignIn(ApplicationUser user, bool isPersistent)
        {
            var authenticationManager = OwinService.GetAuthenticationManager();
            var userManager = OwinService.GetApplicationUserManager();

            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie));
        }

        private void SendAccountConfirmationEmail(ApplicationUser user, string callbackUrl)
        {
            var message = GetAccountConfirmationMessage(user, callbackUrl);

            EmailService.SendEmail(message);
        }

        private MailMessage GetAccountConfirmationMessage(ApplicationUser user, string callbackUrl)
        {
            var template = File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"\Views\Email\ConfirmationEmail.cshtml");
            var emailModel = new ConfirmationMail { CallbackUrl = callbackUrl };
            var body = RazorEngine.Razor.Parse(template, emailModel);
            var subject = "DailyReport | Account Confirmation";

            return EmailService.GetHtmlMessage(user.Email, subject, body);
        }

        #endregion
    }
}
