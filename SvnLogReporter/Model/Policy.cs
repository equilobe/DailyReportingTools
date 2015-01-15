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
        public Uri DraftConfirmationUrl
        {
            get
            {
                var now = DateTime.Now.ToOriginalTimeZone();
                if (AdvancedOptions.NoDraft)
                    return null;

                return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/send/" + ProjectKey + UniqueProjectKey + "?date=" + now.ToString());
            }
        }

        [XmlIgnore]
        public Uri ResendDraftUrl
        {
            get
            {
                var now = DateTime.Now.ToOriginalTimeZone();
                if (AdvancedOptions.NoDraft)
                    return null;

                return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/resendDraft/" + ProjectKey + UniqueProjectKey + "?date=" + now.ToString());
            }
        }

        public DateTime LastReportSentDate { get; set; }

        public string ReportTitle { get; set; }

        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SharedSecret { get; set; }
        public string ProjectKey { get; set; }
        public string ReportTime { get; set; }

        [XmlIgnore]
        public DateTime ReportTimeDateFormat
        {
            get
            {
                if (ReportTime == null)
                    return new DateTime();

                return DateTime.Parse(ReportTime);
            }
        }

        public string Emails { get; set; }
        public string DraftEmails { get; set; }

        public int AllocatedHoursPerMonth { get; set; }
        public int AllocatedHoursPerDay { get; set; }

        public AdvancedOptions AdvancedOptions { get; set; }


        //TODO: delete following 
        public string TargetGroup { get; set; }
        public string ProjectName { get; set; }

        public string PermanentTaskLabel { get; set; }
        public List<string> AdditionalWorkflowStatuses { get; set; }

        public string EmailSubject { get; set; }

        public string UniqueProjectKey { get; set; }

        public string ReopenedStatus { get; set; }
        public bool IsWeekendReportActive { get; set; }

        public SourceControl SourceControl { get; set; }
        public List<User> AuthorsCorrelation { get; set; }
        public List<string> IgnoredAuthors { get; set; }

        public List<Override> Overrides { get; set; }

        public Override CurrentOverride
        {
            get
            {
                if (Overrides == null)
                    return null;

                return Overrides.Find(o => o.Month.ToLower() == DateTime.Now.ToOriginalTimeZone().CurrentMonth().ToLower());
            }
        }

        [XmlIgnore]
        public bool OverrideThisMonth
        {
            get
            {
                if (Overrides == null)
                    return false;

                return Overrides.Exists(o => o.Month.ToLower() == DateTime.Now.ToOriginalTimeZone().CurrentMonth().ToLower());
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
        public List<string> EmailCollection { get; set; }

        public void SetEmailCollection()
        {
            if (!AdvancedOptions.NoDraft)
                EmailCollection = DraftEmails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            else
                EmailCollection = Emails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public void SetIndividualEmail(string emailAdress)
        {
            EmailCollection = new List<string>();
            EmailCollection.Add(emailAdress);
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

        public void SetPermanentTaskLabel()
        {
            this.PermanentTaskLabel = this.PermanentTaskLabel.ToLower();
        }
    }
}
