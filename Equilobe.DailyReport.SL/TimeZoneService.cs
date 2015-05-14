using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Equilobe.DailyReport.SL
{
    class TimeZoneService : ITimeZoneService
    {
        public IConfigurationService ConfigurationService { get; set; }

        public List<Equilobe.DailyReport.Models.TimeZone.TimeZone> GetSystemTimeZones()
        {
            var timeZoneList = new List<Equilobe.DailyReport.Models.TimeZone.TimeZone>();
            TimeZoneInfo.GetSystemTimeZones()
                        .ToList()
                        .ForEach(tz => timeZoneList.Add(new Equilobe.DailyReport.Models.TimeZone.TimeZone()
                        {
                            Id = tz.Id,
                            Name = tz.DisplayName,
                            UtcOffset = tz.BaseUtcOffset.TotalMinutes
                        }));

            return timeZoneList;
        }

        public TimeSpan GetOffsetFromTimezoneId(string timeZoneId)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return timeZoneInfo.GetUtcOffset(DateTime.Now.ToUniversalTime());
        }

        public string GetWindowsTimeZoneIdByIanaTimeZone (ItemContext<string> itemContext)
        {
            var timeZoneMappingPath = ConfigurationService.GetTimeZoneMappingPath();
            XmlDocument timeZoneMap = GetTimeZoneMapXml(timeZoneMappingPath);
            XmlNode mapZoneNode = timeZoneMap.SelectSingleNode(String.Format("//mapZone[@type='{0}']", itemContext.Id));
            return mapZoneNode == null ? null : mapZoneNode.Attributes["other"].Value.Trim();
        }

        private static XmlDocument GetTimeZoneMapXml(string timeZoneMappingPath)
        {
            var mapFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, timeZoneMappingPath);
            XmlDocument doc = new XmlDocument();
            doc.Load(mapFilePath);
            return doc;
        }
    }
}
