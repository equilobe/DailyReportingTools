using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class ProjectController : ApiController
    {
        public IDataService DataService { get; set; }
        public ISettingsService SettingsService { get; set; }
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

        public List<BasicReportSettings> Get(long id)
        {
            return SettingsService.GetAllBasicSettings(new ItemContext(id));
        }
    }
}
