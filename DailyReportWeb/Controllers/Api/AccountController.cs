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
		[Route("Register")]
		public async Task<HttpResponseMessage> Register(RegisterModel model)
		{
			List<string> errors = new List<string>();
			errors = Check(ModelState);

			if(errors.Count != 0)
				return Request.CreateResponse(HttpStatusCode.NotAcceptable, errors);

			var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };
			IdentityResult result = await UserManager.CreateAsync(user, model.Password);

			if(!result.Succeeded)
				return Request.CreateResponse(HttpStatusCode.NotAcceptable, result.Errors);

			await SignInAsync(user, isPersistent: false);
			// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
			// Send an email with this link
			//string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
			//var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
			//await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

			return Request.CreateResponse(HttpStatusCode.OK);
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

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error);
			}
		}

		//Created to remove duplicated error message generation when a bad model is passed to a service.  
		//This method recieves the ModelState, loops through all the erros, and builds them into a list.
		//An empty list means no errors.
		public static List<string> Check(System.Web.Http.ModelBinding.ModelStateDictionary ModelState)
		{
			var modelStateErrors = ModelState.Values;
			List<string> errors = new List<string>();
			foreach (var s in modelStateErrors)
				foreach (var e in s.Errors)
					if (e.ErrorMessage != null && e.ErrorMessage.Trim() != "")
						errors.Add(e.ErrorMessage);

			return errors;
		}
    }
}
