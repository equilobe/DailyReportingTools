using Equilobe.DailyReport.Models.Interfaces;
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
using Equilobe.DailyReport.Models;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class ProjectController : ApiController
    {
        public IDataService DataService { get; set; }
        public IPolicySummaryService PolicySummaryService { get; set; }
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

        public List<Instance> Get()
        {
			string userId = User.Identity.GetUserId();
			var currentUser = UserManager.FindById(userId);

			return DataService.GetInstances(currentUser);
        }
        
        public List<ReportSettingsSummary> Get(long id)
        {           
            return PolicySummaryService.GetPoliciesSummary(new ItemContext(id));
        }
    }
}
