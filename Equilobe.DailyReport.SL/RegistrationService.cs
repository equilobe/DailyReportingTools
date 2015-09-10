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

            var callbackUrl = GetConfirmationEmailCallbackUrl(user.Id);
            SendAccountConfirmationEmail(user, callbackUrl);

            DataService.SaveInstance(model);

            return SimpleResult.Success("Account confirmation details has been sent to your mail.");
        }

        public SimpleResult ConfirmEmail(EmailConfirmation emailConfirmation)
        {
            var userManager = OwinService.GetApplicationUserManager();
            var user = userManager.FindById(emailConfirmation.UserId);
            var code = HttpUtility.UrlDecode(emailConfirmation.Code);
            var errorMessage = "Invalid activation token.";

            var validate = ValidateUserToken(emailConfirmation, user, errorMessage);
            if (validate.HasError)
                return validate;

            if (user.EmailConfirmed)
                return SimpleResult.Error("Your account was already activated.");

            IdentityResult result = userManager.ConfirmEmail(emailConfirmation.UserId, code);
            if (!result.Succeeded)
                return SimpleResult.Error(result.Errors.First());

            var instanceId = userManager.FindById(emailConfirmation.UserId)
                                        .InstalledInstances
                                        .Single()
                                        .Id;
            SettingsService.SyncAllBasicSettings(new ItemContext(instanceId));

            return SimpleResult.Success("Your account was activated. You can now sign in.");
        }

        public SimpleResult ResetPassword(EmailConfirmation emailConfirmation)
        {
            var userManager = OwinService.GetApplicationUserManager();
            var user = userManager.FindById(emailConfirmation.UserId);
            var errorMessage = "Invalid password reset token.";

            return ValidateUserToken(emailConfirmation, user, errorMessage);
        }

        public SimpleResult ChangePassword(ResetPasswordModel passwordModel)
        {
            var userManager = OwinService.GetApplicationUserManager();
            var user = userManager.FindById(passwordModel.UserId);
            var code = HttpUtility.UrlDecode(passwordModel.UserToken);

            if (user == null)
                return SimpleResult.Error("Invalid token");

            IdentityResult result = userManager.ResetPassword(passwordModel.UserId, passwordModel.UserToken, passwordModel.NewPassword);
            if (!result.Succeeded)
                return SimpleResult.Error(result.Errors.First());

            return SimpleResult.Success("Password successfuly changed. You can sign in now.");
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

        public bool IsTrialAvailableForInstance(long instanceId)
        {
            var user = DataService.GetUser(instanceId);
            var instance = DataService.GetInstance(instanceId);

            if (user.InstalledInstances.First().Id == instanceId && instance.Subscriptions.IsEmpty())
                return true;

            return false;
        }

        public void ValidateJiraDetails(RegisterModel model)
        {
            if (!Validations.Url(model.BaseUrl))
                throw new ArgumentException();

            var credentialsValid = JiraService.CredentialsValid(model, false);
            if (!credentialsValid)
                throw new ArgumentException();
        }

        public SimpleResult SendResetPasswordEmail(string email)
        {
            var userManager = OwinService.GetApplicationUserManager();
            var user = userManager.FindByEmail(email);
            if (user == null)
                return SimpleResult.Error("There is no user registered with this email adress");

            if (!user.EmailConfirmed)
                return SimpleResult.Error("The email adress has not been confirmed. Please confirm your email first!");

            string token = userManager.GeneratePasswordResetToken(user.Id);
            var callbackUrl = GetResetPasswordCallbackUrl(user.Id);
            SendPasswordResetEmail(user, callbackUrl);

            return SimpleResult.Success("Details for resetting password have been sent to your email adress");
        }

        #region Helpers

        private SimpleResult ValidateUserToken(EmailConfirmation emailConfirmation, ApplicationUser user, string errorMessage)
        {
            var code = HttpUtility.UrlDecode(emailConfirmation.Code);

            if (emailConfirmation.UserId == null || code == null)
                return SimpleResult.Error(errorMessage);

            if (user == null)
                return SimpleResult.Error(errorMessage);

            return SimpleResult.Success(string.Empty);
        }

        private string GetConfirmationEmailCallbackUrl(string userId)
        {
            string code = HttpUtility.UrlEncode(OwinService.GetApplicationUserManager().GenerateEmailConfirmationToken(userId));

            return string.Format("{0}/app/confirmEmail?userId={1}&code={2}", ConfigurationService.GetWebBaseUrl(), userId, code);
        }

        private string GetResetPasswordCallbackUrl(string userId)
        {
            string code = HttpUtility.UrlEncode(OwinService.GetApplicationUserManager().GeneratePasswordResetToken(userId));

            return string.Format("{0}/app/resetPassword?userId={1}&code={2}", ConfigurationService.GetWebBaseUrl(), userId, code);
        }

        private void ValidateRegisterModel(RegisterModel model)
        {
            if (!Validations.Mail(model.Email) ||
                !Validations.Password(model.Password) ||
                !Validations.Url(model.BaseUrl))
                throw new ArgumentException();
        }

        private void ValidateMail(LoginModel model)
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

        private void SendPasswordResetEmail(ApplicationUser user, string callbackUrl)
        {
            var message = GetPasswordResetMessage(user, callbackUrl);

            EmailService.SendEmail(message);
        }

        private MailMessage GetEmailMessage(EmailContext context)
        {
            var template = File.ReadAllText(context.ViewPath);
            var body = RazorEngine.Razor.Parse(template, context.Email);

            return EmailService.GetHtmlMessage(context.UserEmail, context.Subject, body);
        }

        private EmailContext GetEmailContext(string callbackUrl, string userEmail, string viewPath, string subject)
        {
            return new EmailContext
            {
                Email = new Email
                {
                    CallbackUrl = callbackUrl
                },
                UserEmail = userEmail,
                Subject = subject,
                ViewPath = viewPath
            };
        }

        private MailMessage GetAccountConfirmationMessage(ApplicationUser user, string callbackUrl)
        {
            var viewPath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Views\Email\ConfirmationEmail.cshtml";
            var subject = "DailyReport | Account Confirmation";
            var emailContext = GetEmailContext(callbackUrl, user.Email, viewPath, subject);

            return GetEmailMessage(emailContext);
        }

        private MailMessage GetPasswordResetMessage(ApplicationUser user, string callbackUrl)
        {
            var viewPath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Views\Email\PasswordResetEmail.cshtml";
            var subject = "DailyReport | Account Confirmation";
            var emailContext = GetEmailContext(callbackUrl, user.Email, viewPath, subject);

            return GetEmailMessage(emailContext);
        }

        #endregion
    }
}
