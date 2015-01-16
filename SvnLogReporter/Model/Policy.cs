using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [XmlIgnore]
        public Uri DraftConfirmationUrl
        {
            get
            {
                var now = DateTime.Now.ToOriginalTimeZone();
                if (AdvancedOptions.NoDraft)
                    return null;

                return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/send/" + ProjectKey + GeneratedProperties.UniqueProjectKey + "?date=" + now.ToString());
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

                return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/resendDraft/" + ProjectKey + GeneratedProperties.UniqueProjectKey + "?date=" + now.ToString());
            }
        }

        //Base Properties
        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SharedSecret { get; set; }
        public string ProjectKey { get; set; } // check for id 
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


        [DefaultValue(0)]
        public int AllocatedHoursPerMonth { get; set; }
        [DefaultValue(0)]
        public int AllocatedHoursPerDay { get; set; }

        //TODO: delete following 
        public string TargetGroup { get; set; }
        public string ProjectName { get; set; }
        public bool IsWeekendReportActive { get; set; }

        public AdvancedOptions AdvancedOptions { get; set; }

        public List<Month> MonthlyOptions { get; set; }

        public SourceControl SourceControl { get; set; }

        public List<User> UserOptions { get; set; }

        public GeneratedProperties GeneratedProperties { get; set; }
       
        [XmlIgnore]
        public Month CurrentOverride
        {
            get
            {
                if (MonthlyOptions == null)
                    return null;

                return MonthlyOptions.Find(o => o.MonthName.ToLower() == DateTime.Now.ToOriginalTimeZone().CurrentMonth().ToLower());
            }
        }

        [XmlIgnore]
        public bool OverrideThisMonth
        {
            get
            {
                if (MonthlyOptions == null)
                    return false;

                return MonthlyOptions.Exists(o => o.MonthName.ToLower() == DateTime.Now.ToOriginalTimeZone().CurrentMonth().ToLower());
            }
        }


        public IDictionary<string, string> Users
        {
            get
            {
                return UserOptions.ToDictionary(d => d.JiraUserKey, d => d.SourceControlAuthor);
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
            this.AdvancedOptions.PermanentTaskLabel = this.AdvancedOptions.PermanentTaskLabel.ToLower();
        }

        public void SetDefaultProperties()
        {
            if (AdvancedOptions.ReportTitle == null)
                AdvancedOptions.ReportTitle = ProjectName + " Daily Report";
            if (GeneratedProperties.RootPath == null)
                GeneratedProperties.RootPath = Path.GetFullPath(ProjectName);
        }
    }
}
