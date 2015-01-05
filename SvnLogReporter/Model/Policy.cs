using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
namespace SourceControlLogReporter.Model
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
        [XmlIgnore]
        public bool IsDraft { get; set; }
        [XmlIgnore]
        public Uri DraftConfirmationUrl
        {
            get
            {
                var now = DateTime.Now.ToOriginalTimeZone();
                if (IsDraft == true)
                    return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/send/" + ProjectKey + UniqueProjectKey + "?date=" + now.ToString());
                else
                    return null;
            }
        }

        [XmlIgnore]
        public Uri ResendDraftUrl
        {
            get
            {
                var now = DateTime.Now.ToOriginalTimeZone();
                if (IsDraft == true)
                    return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/resendDraft/" + ProjectKey + UniqueProjectKey + "draft" + "?date=" + now.ToString());
                else
                    return null;
            }
        }

        public DateTime LastReportSentDate { get; set; }

        public string ReportTitle { get; set; }
        public string ReportTime { get; set; }
        [XmlIgnore]
        public DateTime ReportTimeDateFormat
        {
            get
            {
                if (ReportTime != null)
                    return DateTime.Parse(ReportTime);
                else
                    return new DateTime();
            }
        }
        public string DraftTime { get; set; }
        [XmlIgnore]
        public DateTime DraftTimeDateFormat
        {
            get
            {
                if (DraftTime != null)
                    return DateTime.Parse(DraftTime);
                else
                    return new DateTime();
            }
        }
        public string BaseUrl { get; set; }
        public string TargetGroup { get; set; }
        public string PermanentTaskLabel { get; set; }
        public List<string> AdditionalWorkflowStatuses { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Emails { get; set; }
        public string DraftEmails { get; set; }
        public string EmailSubject { get; set; }
        public string ProjectKey { get; set; }
        public string UniqueProjectKey { get; set; }
        public string ProjectName { get; set; }
        public string ReopenedStatus { get; set; }
        public bool IsWeekendReportActive { get; set; }
        public int AllocatedHoursPerMonth { get; set; }
        public int AllocatedHoursPerDay { get; set; }
        public SourceControl SourceControl { get; set; }
        public List<User> AuthorsCorrelation { get; set; }
        public List<string> IgnoredAuthors { get; set; }

        public List<Override> Overrides { get; set; }

        [XmlIgnore]
        public List<Override> CurrentOverrides { get; set; }

        public Override CurrentOverride
        {
            get
            {
                if (Overrides != null)
                    return Overrides.Find(o => o.Month.ToLower() == DateTime.Now.ToOriginalTimeZone().CurrentMonth().ToLower());
                else
                    return null;
            }
        }

        [XmlIgnore]
        public bool OverrideThisMonth
        {
            get
            {
                if (Overrides != null)
                    return Overrides.Exists(o => o.Month.ToLower() == DateTime.Now.ToOriginalTimeZone().CurrentMonth().ToLower());
                else
                    return false;
            }
        }


        public IDictionary<string, string> Users
        {
            get
            {
                return AuthorsCorrelation.ToDictionary(d => d.JiraAuthor, d => d.SourceControlAuthor);
            }
        }

        [XmlIgnore]
        public IEnumerable<string> EmailCollection
        {
            get
            {
                if (IsDraft == true)
                    return DraftEmails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                else
                    return Emails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static Policy CreateFromFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Policy));
                return (Policy)ser.Deserialize(fs);
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

        public void WriteDateToPolicy(string filePath, DateTime date)
        {
            XDocument xDocument = XDocument.Load(filePath);
            XElement root = xDocument.Element("Policy");
            if (root.Elements("LastReportSentDate").Any())
                root.Elements("LastReportSentDate").Remove();
            root.AddFirst(new XElement("LastReportSentDate", date));
            xDocument.Save(filePath);
        }

        public void SetPermanentTaskLabel()
        {
            this.PermanentTaskLabel = this.PermanentTaskLabel.ToLower();
        }

        public void SetDraftMode(Options options)
        {
            if (options.IsDraft == true)
            {
                this.ReportTitle = "DRAFT " + this.ReportTitle;
                IsDraft = true;
            }
        }

        public void SetCurrentOverride(Options options)
        {
            var day = options.FromDate;
            if(Overrides!=null)
                while (day < options.ToDate)
                {
                    if (Override.SearchOverride(CurrentOverrides, day) == null)
                    {
                        var currentOverride = Override.SearchOverride(Overrides, day);
                        AddOverride(currentOverride);
                    }
                    day = day.AddDays(1);
                }
        }

        private void AddOverride(Override currentOverride)
        {
            if (currentOverride != null)
            {
                if (CurrentOverrides == null)
                    CurrentOverrides = new List<Override>();
                CurrentOverrides.Add(currentOverride);
            }
        }
    }
}
