using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using Owin;
using System.Net;
using System.Linq;
using System.Web.Routing; 

namespace DailyReportWeb.Controllers.Api
{
	[Authorize]
    public class AccountController : ApiController
    {
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
			List<string> errors = new List<string>();

			if (errors.Count != 0)
			{
				return new AccountResponse() { 
					Success= false,
					ErrorList = errors,
					Message = "Correct errors first."
				};
			}

			var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };
			IdentityResult result = await UserManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
				return new AccountResponse(){
					Success=false,
					Message = "User could not be created.",
					ErrorList = result.Errors.ToList()
				};

			// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
			string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            var newRouteValues = new RouteValueDictionary(new { userId = user.Id, code = code });
            newRouteValues.Add("httproute", true);
            System.Web.Mvc.UrlHelper urlHelper = new System.Web.Mvc.UrlHelper(HttpContext.Current.Request.RequestContext, RouteTable.Routes);
            string callbackUrl = urlHelper.Action(
                "ConfirmEmail",
                "Account",
                newRouteValues,
                HttpContext.Current.Request.Url.Scheme
                );

			await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

			return new AccountResponse(){
				Success = true,
                Message = "A message has been sent to your mail. Please confirm the account."
			};
		}

        [AllowAnonymous]
        [HttpGet]
        public async Task<AccountResponse> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return new AccountResponse() { Success = false };

            IdentityResult result = await UserManager.ConfirmEmailAsync(userId, code);
            if (!result.Succeeded)
                return new AccountResponse() { 
                    Success = false, 
                    ErrorList = result.Errors.ToList() 
                };

            return new AccountResponse() { Success = true };
        }

		[AllowAnonymous]
		public async Task<AccountResponse> Login(LoginModel model)
		{
			List<string> errors = new List<string>();
			if (errors.Count != 0)
				return new AccountResponse() {
					Success = false,
					ErrorList = errors,
					Message = "Correct errors first."
				};

			var user = await UserManager.FindAsync(model.Email, model.Password);

            if(!await UserManager.IsEmailConfirmedAsync(user.Id))
                return new AccountResponse(){
                    Success = false,
                    Message = "Account has not been activated yet. Please check the mail and proceed with account confirmation."
                };

			if(user==null)
				return new AccountResponse() {
					Success = false,
					ErrorList = errors,
					Message = "Invalid username or password."
				};

			await SignInAsync(user, model.RememberMe);
			return new AccountResponse(){
				Success = true
			};
		}

		public AccountResponse LogOff()
		{
			AuthenticationManager.SignOut();
			return new AccountResponse(){
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
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
			AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
		}

    }
}
