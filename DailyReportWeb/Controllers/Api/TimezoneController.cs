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
        public List<Equilobe.DailyReport.Models.Web.TimeZone> Get()
        {
            var timeZoneList = new List<Equilobe.DailyReport.Models.Web.TimeZone>();
            TimeZoneInfo.GetSystemTimeZones()
                        .ToList()
                        .ForEach(tz => timeZoneList.Add(new Equilobe.DailyReport.Models.Web.TimeZone()
                        {
                            Id = tz.Id,
                            Name = tz.DisplayName
                        }));

            return timeZoneList;
        }
    }
}
