using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DailyReportWeb.Models;
using SourceControlLogReporter.Model;

namespace DailyReportWeb.Controllers.Api
{
    public class PolicyController : ApiController
    {
        // GET: api/Policy
        public IEnumerable<PolicySummary> Get()
        {
            return new PolicySummary[] 
            {
                new PolicySummary { Id= "asdf1", ProjectName="Alyssa", ReportTime = "09:00 AM" },
                new PolicySummary { Id= "asdf2", ProjectName="Board Prospects" },
                new PolicySummary { Id= "asdf3", ProjectName="Daily Report Tool ", ReportTime = "09:00 AM" },
            };
        }

        // GET: api/Policy/5
        public Policy Get(string id)
        {
            return new Policy();
        }

        // Policies are not created directly!
        // POST: api/Policy
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT: api/Policy/5
        public void Put(string id, [FromBody]Policy updatedPolicy)
        {
            // TODO: save the updated policy
        }

        // Policies are not deleted directly!
        // DELETE: api/Policy/5
        //public void Delete(int id)
        //{
        //}
    }
}
