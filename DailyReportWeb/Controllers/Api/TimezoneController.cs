using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    public class TimezoneController : ApiController
    {
        public ITimeZoneService TimeZoneService { get; set; }

        public List<Equilobe.DailyReport.Models.TimeZone.TimeZone> Get()
        {
            return TimeZoneService.GetSystemTimeZones();
        }

        /// <summary>
        /// </summary>
        /// <param name="id">The id is the IANA time zone returned from the ajax call made to freegeoip.net. 
        /// NOTE: the id has all of the slashes ('/') replaced by '-'</param>
        /// <returns></returns>
        public Equilobe.DailyReport.Models.TimeZone.TimeZones Get(string id)
        {
            id = id.Replace('-', '/');
            return new Equilobe.DailyReport.Models.TimeZone.TimeZones()
            {
                TimeZoneList = TimeZoneService.GetSystemTimeZones(),
                SuggestedTimeZone = TimeZoneService.GetWindowsTimeZoneIdByIanaTimeZone(new ItemContext<string>(id))
            };
        } 
    }
}
