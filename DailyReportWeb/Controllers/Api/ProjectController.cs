using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.SL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class ProjectController : ApiController
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

        public List<InstalledInstance> Get()
        {
			string userId = User.Identity.GetUserId();
			
			var currentUser = UserManager.FindById(userId);
			return currentUser.InstalledInstances.ToList();

			//var username = User.GetUsername();
			//return new DataService().GetInstances(username);
        }

        public IEnumerable<PolicySummary> Get(long id)
        {
            var username = User.GetUsername();
            var baseUrl = new DataService().GetBaseUrl(id);

            var requestContext = new JiraRequestContext
            {
                BaseUrl = baseUrl,
                Username = username,
                SharedSecret = new DataService().GetSharedSecret(baseUrl),
                Password = new DataService().GetPassword(baseUrl, username)
            };

            return new PolicySummaryService(requestContext).GetPoliciesSummary();
        }
    }
}
