using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.DAL;
using System.Net.Mail;

namespace DailyReportWeb
{
	// Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

	public class ApplicationUserManager : UserManager<ApplicationUser>
	{
		public ApplicationUserManager(IUserStore<ApplicationUser> store)
			: base(store)
		{
		}

		public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
		{
			var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ReportsDb>()));
			// Configure validation logic for usernames
			manager.UserValidator = new UserValidator<ApplicationUser>(manager)
			{
				AllowOnlyAlphanumericUserNames = false,
				RequireUniqueEmail = true
			};
			// Configure validation logic for passwords
			manager.PasswordValidator = new PasswordValidator
			{
				RequiredLength = 6,
				RequireNonLetterOrDigit = true,
				RequireDigit = true,
				RequireLowercase = true,
				RequireUppercase = true,
			};
			// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
			// You can write your own provider and plug in here.
			manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
			{
				Subject = "Security Code",
				BodyFormat = "Your security code is: {0}"
			});
			manager.EmailService = new EmailService();
			var dataProtectionProvider = options.DataProtectionProvider;
			if (dataProtectionProvider != null)
			{
				manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
			}
			return manager;
		}
	}

	public class EmailService : IIdentityMessageService
	{
		public Task SendAsync(IdentityMessage message)
		{
            var client = new SmtpClient() { EnableSsl = true };
            var credential = (System.Net.NetworkCredential)client.Credentials;
			var mail = new MailMessage(credential.UserName, message.Destination);
			mail.Subject = message.Subject;
			mail.Body = message.Body;
            mail.IsBodyHtml = true;

			return client.SendMailAsync(mail);
		}
	}
}