using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ITimeZoneService : IService
    {
        /// <summary>
        /// Maps the IANA time zone with SystemTimeZoneId. Retrns an empty string if there is no equivalent
        /// </summary>
        /// <param name="ianaTimeZone"></param>
        /// <returns>SystemTimeZoneId</returns>
        string GetWindowsTimeZoneIdByIanaTimeZone(string ianaTimeZone);
    }
}
