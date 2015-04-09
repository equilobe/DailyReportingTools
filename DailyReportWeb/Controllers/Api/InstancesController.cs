using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class InstancesController : ApiController
    {
        public IDataService DataService { get; set; }
        public IJiraService JiraService { get; set; }

        public List<Instance> Get()
        {
            return DataService.GetInstances();
        }

        public List<Instance> Post([FromBody]RegisterModel instance)
        {
            if (!Validations.Url(instance.BaseUrl))
                throw new ArgumentException();

            var credentialsValid = JiraService.CredentialsValid(instance, false);
            if (!credentialsValid)
                throw new ArgumentException();

            instance.Email = User.GetUsername();
            DataService.SaveInstance(instance);

            return DataService.GetInstances();
        }
    }
}
