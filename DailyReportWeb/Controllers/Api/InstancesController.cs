using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.TaskScheduling;
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
        public ITaskSchedulerService TaskSchedulerService { get; set; }
        public IRegistrationService RegistrationService { get; set; }

        public List<Instance> Get()
        {
            return DataService.GetInstances();
        }

        public RegisterModel CheckInstanceCredentials([FromBody]RegisterModel instance)
        {
            if (!Validations.Url(instance.BaseUrl))
                throw new ArgumentException();

            var credentialsValid = JiraService.CredentialsValid(instance, false);
            if (!credentialsValid)
                throw new ArgumentException();

            instance.Email = User.GetUsername();
            //   DataService.SaveInstance(instance); // will be modified

            return instance;
        }

        public bool Get(long id)
        {
            return RegistrationService.IsTrialAvailableForInstance(id);
        }

        //public List<Instance> Delete(long id)
        //{
        //    var projectKeys = DataService.GetUniqueProjectsKey(id);
        //    TaskSchedulerService.DeleteMultipleTasks(new ProjectListContext
        //    {
        //        UniqueProjectKeys = projectKeys
        //    });

        //    DataService.DeleteInstance(id);

        //    return DataService.GetInstances();
        //}
    }
}
