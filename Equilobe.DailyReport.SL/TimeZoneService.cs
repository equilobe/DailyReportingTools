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
        private static string TimeZoneMappingFileName = "TimeZoneMappings.xml";

        public List<Equilobe.DailyReport.Models.Web.TimeZone> GetSystemTimeZones()
        {
            var timeZoneList = new List<Equilobe.DailyReport.Models.Web.TimeZone>();
            TimeZoneInfo.GetSystemTimeZones()
                        .ToList()
                        .ForEach(tz => timeZoneList.Add(new Equilobe.DailyReport.Models.Web.TimeZone()
                        {
                            Id = tz.Id,
                            Name = tz.DisplayName,
                            UtcOffset = tz.BaseUtcOffset.TotalMinutes
                        }));

            return timeZoneList;
        }

        public string GetWindowsTimeZoneIdByIanaTimeZone (string ianaTimeZone)
        {
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            var mapFilePath = Path.Combine(path, TimeZoneMappingFileName);

            XmlDocument timeZoneMap = GetTimeZoneMapXml(mapFilePath);
            XmlNode mapZoneNode = timeZoneMap.SelectSingleNode(String.Format("//mapZone[@type='{0}']", ianaTimeZone));
            return mapZoneNode == null ? String.Empty : mapZoneNode.Attributes["other"].Value.Trim();
        }

        private static XmlDocument GetTimeZoneMapXml(string mapPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(mapPath);
            return doc;
        }
    }
}
