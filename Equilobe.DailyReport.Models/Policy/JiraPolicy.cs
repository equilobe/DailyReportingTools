﻿using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.Policy
{
    public class JiraPolicy
    {
        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SharedSecret { get; set; }

        public long ProjectId { get; set; }
        public string DraftEmails { get; set; }
        public string Emails { get; set; }
        public string ReportTime { get; set; }

        [XmlIgnore]
        public List<string> EmailCollection { get; set; }


        [DefaultValue(0)]
        public int AllocatedHoursPerMonth { get; set; }
        [DefaultValue(0)]
        public int AllocatedHoursPerDay { get; set; }

        public SourceControlOptions SourceControlOptions { get; set; }

        public AdvancedOptions AdvancedOptions { get; set; }

        public List<Month> MonthlyOptions { get; set; }

        public List<User> UserOptions { get; set; }

        [XmlIgnore]
        public Month CurrentOverride { get; set; }

        [XmlIgnore]
        public bool IsThisMonthOverriden { get; set; }

        [XmlIgnore]
        public IDictionary<string, List<string>> Users { get; set; }

        public JiraPolicy()
        {

        }
    }
}
