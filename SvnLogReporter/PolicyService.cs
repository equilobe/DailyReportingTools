using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourceControlLogReporter
{
    public class PolicyService
    {
        Policy Policy { get; set; }

        public PolicyService(Policy policy)
        {
            Policy = policy;
        }

        public void SetPolicy()
        {
            Policy.ReportTimeDateFormat = GetDateTimeFromString(Policy.ReportTime);
            SetEmailCollection();
        }

        private static DateTime GetDateTimeFromString(string date)
        {
            if (date == null)
                return new DateTime();

            return DateTime.Parse(date);
        }

        private void SetEmailCollection()
        {
            Policy.EmailCollection = Policy.Emails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static Policy LoadFromFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Policy));
                return (Policy)ser.Deserialize(fs);
            }
        }

        public static void SaveToFile(string filePath, Policy policy)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Policy));
                ser.Serialize(fs, policy);
            }
        }
    }
}
