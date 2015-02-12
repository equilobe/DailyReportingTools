using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.ReportPolicy
{
    public class JiraPolicy
    {
        //Base Properties
        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SharedSecret { get; set; }

        public int ProjectId { get; set; }
        public string DraftEmails { get; set; }
        public string Emails { get; set; }
        public string ReportTime { get; set; }
        [XmlIgnore]
        public DateTime ReportTimeDateFormat { get; set; }

        [XmlIgnore]
        public List<string> EmailCollection { get; set; }


        [DefaultValue(0)]
        public int AllocatedHoursPerMonth { get; set; }
        [DefaultValue(0)]
        public int AllocatedHoursPerDay { get; set; }

        public SourceControlOptions SourceControlOptions { get; set; }

        public JiraAdvancedOptions AdvancedOptions { get; set; }

        public List<Month> MonthlyOptions { get; set; }

        public List<User> UserOptions { get; set; }

        public JiraGeneratedProperties GeneratedProperties { get; set; }

        [XmlIgnore]
        public Month CurrentOverride { get; set; }

        [XmlIgnore]
        public bool IsThisMonthOverriden { get; set; }

        [XmlIgnore]
        public IDictionary<string, List<string>> Users { get; set; }


        private static TimeSpan? _offsetFromUtc;
        [XmlIgnore]
        private static TimeSpan OffsetFromUtc
        {
            get
            {
                if (!_offsetFromUtc.HasValue)
                    throw new ApplicationException("You must first initialize the Offset");
                return _offsetFromUtc.Value;
            }
            set
            {
                _offsetFromUtc = value;
            }
        }
    }
}
