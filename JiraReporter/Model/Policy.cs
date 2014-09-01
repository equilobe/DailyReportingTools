﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JiraReporter.Model
{
    public class Policy
    {
        string _rootPath = string.Empty;
        public string RootPath { get { return Path.GetFullPath(_rootPath); } set { _rootPath = value; } }

        [XmlIgnore]
        public string LogPath { get { return Path.Combine(RootPath, "Logs"); } }
        [XmlIgnore]
        public string LogArchivePath { get { return Path.Combine(RootPath, "LogArchive"); } }
        [XmlIgnore]
        public string ReportsPath { get { return Path.Combine(RootPath, "Reports"); } }
        [XmlIgnore]
        public string UnsentReportsPath { get { return Path.Combine(RootPath, "UnsentReports"); } }

        public int DayStartHour { get; set; }

        public string ReportTitle { get; set; }
        public string BaseUrl { get; set; }
        public string TargetGroup { get; set; }
        public string PermanentTaskLabel { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Emails { get; set; }

        [XmlIgnore]
        public IEnumerable<string> EmailCollection
        {
            get
            {
                return Emails.Split(new char[] { ' ',  ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static Policy CreateFromFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Policy));
                return (Policy) ser.Deserialize(fs);
            }
        }

        public void SaveToFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Policy));
                ser.Serialize(fs, this);
            }
        }
    }
    
}
