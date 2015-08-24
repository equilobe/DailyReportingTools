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

namespace Equilobe.DailyReport.SL
{
    public class RegistrationService : IRegistrationService
    {
        public IJiraService JiraService { get; set; }
        public IDataService DataService { get; set; }
        public ISettingsService SettingsService { get; set; }
        public IConfigurationService ConfigurationService { get; set; }

        public SimpleResult RegisterUser(RegisterModel model, UserManager<ApplicationUser> userManager)
        {
            try
            {
                ValidateRegisterModel(model);
            }
            catch
            {
                return SimpleResult.Error(ApplicationErrors.ValidationError);
            }

            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email
            };

            var searchedUser = SearchUser(model);
            if (searchedUser != null)
                return SimpleResult.Error(ApplicationErrors.UserAlreadyCreated);

            IdentityResult result = userManager.Create(user, model.Password);
            
            if (!result.Succeeded)
                return SimpleResult.Error(result.Errors.First());

            DataService.SaveInstance(model);

            //When testing, do not send emails
        //    var callbackUrl = GetCallbackUrl(userManager, user.Id);
         //   SendAccountConfirmationEmail(user, callbackUrl);

            return SimpleResult.Success("");
        }

        public SimpleResult CheckRegistrationDetails(RegisterModel model, UserManager<ApplicationUser> userManager)
        {
            ValidateRegisterModel(model);

            var credentialsValid = JiraService.CredentialsValid(model, false);
            if (!credentialsValid)
                return SimpleResult.Error(ApplicationErrors.InvalidJiraCredentials);

            var user = SearchUser(model);

            if (user != null)
                return SimpleResult.Error(ApplicationErrors.EmailNotAvailable(model.Email));

            return SimpleResult.Success("Subscribe to finalize account registration and login after you confirm the account activation email."); 
        }

        private static ApplicationUser SearchUser(RegisterModel model)
        {
            using (var db = new ReportsDb())
            {
                var user = db.Users.SingleOrDefault(u => u.Email == model.Email);
                return user;
            }
        }

        public SimpleResult ConfirmEmail(EmailConfirmation emailConfirmation, UserManager<ApplicationUser> userManager)
        {
            string userId = emailConfirmation.userId;
            string code = HttpUtility.UrlDecode(emailConfirmation.code);

            if (userId == null || code == null)
                return SimpleResult.Error("Invalid activation token.");

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

        public SimpleResult Login(LoginModel model, UserManager<ApplicationUser> userManager)
        {
            ValidateMail(model);

            var user = userManager.Find(model.Email, model.Password);

            if (user == null || userManager.FindByEmail(model.Email) == null)
                return SimpleResult.Error("Invalid username or password.");

            if (!user.EmailConfirmed)
                return SimpleResult.Error("Account has not been activated yet.");

            SignIn(user, userManager, model.RememberMe);

            return SimpleResult.Success("");
        }

        public void Logout()
        {
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;

            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        #region Helpers

        private string GetCallbackUrl(UserManager<ApplicationUser> userManager, string userId)
        {
            string code = HttpUtility.UrlEncode(userManager.GenerateEmailConfirmationToken(userId));

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

        private void SignIn(ApplicationUser user, UserManager<ApplicationUser> userManager, bool isPersistent)
        {
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;

            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie));
        }

        private void SendAccountConfirmationEmail(ApplicationUser user, string callbackUrl)
        {
            var message = GetAccountConfirmationMessage(user, callbackUrl);

            SendEmail(message);
        }

        private static void SendEmail(MailMessage message)
        {
            var smtp = new SmtpClient();

            smtp.Send(message);
        }

        private static MailMessage GetAccountConfirmationMessage(ApplicationUser user, string callbackUrl)
        {
            var template = File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"\Views\Email\ConfirmationEmail.cshtml"); 
            var emailModel = new ConfirmationMail { CallbackUrl = callbackUrl };
            var email = RazorEngine.Razor.Parse(template, emailModel);
            var message = new MailMessage
            {
                Subject = "DailyReport | Account Confirmation",
                Body = email,
                IsBodyHtml = true
            };
            message.To.Add(user.Email);
            return message;
        }

        #endregion
    }
}
