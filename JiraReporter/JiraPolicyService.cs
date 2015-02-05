using Equilobe.DailyReport.Models.ReportPolicy;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JiraReporter
{
    class JiraPolicyService
    {
        JiraPolicy Policy { get; set; }

        public JiraPolicyService(JiraPolicy policy)
        {
            Policy = policy;
        }

        public void SetPolicy(Options options)
        {
            SetUrls();
            Policy.ReportTimeDateFormat = GetDateTimeFromString(Policy.ReportTime);
            Policy.CurrentOverride = GetCurrentOverride();
            Policy.IsThisMonthOverriden = IsThisMonthOverriden();
            Policy.Users = GetUsersDictionary();
            Policy.AdvancedOptions.WeekendDaysList = GetWeekendDays();
            SetMonthlyNonWorkingDays();

            SetDefaultProperties(options);

        }

        private void SetMonthlyNonWorkingDays()
        {
            if (Policy.MonthlyOptions != null)
                foreach (var month in Policy.MonthlyOptions)
                    month.NonWorkingDaysList = MonthlyOptionsHelpers.GetNonWorkingDays(month);
        }

        private void SetUrls()
        {
            Policy.DraftConfirmationUrl = GetDraftConfirmationUrl();
            Policy.ResendDraftUrl = GetResendDraftUrl();
            Policy.IndividualDraftConfirmationUrl = GetIndividualDraftConfirmationUrl();
            Policy.ResendIndividualDraftUrl = GetResendIndividualDraftUrl();
        }

        private Uri GetDraftConfirmationUrl()
        {
            var now = DateTime.Now.ToOriginalTimeZone();
            if (Policy.AdvancedOptions.NoDraft)
                return null;

            return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/send/" + Policy.GeneratedProperties.ProjectKey + Policy.GeneratedProperties.UniqueProjectKey + "?date=" + now.ToString());
        }

        private Uri GetResendDraftUrl()
        {
            var now = DateTime.Now.ToOriginalTimeZone();
            if (Policy.AdvancedOptions.NoDraft)
                return null;

            return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/resendDraft/" + Policy.GeneratedProperties.ProjectKey + Policy.GeneratedProperties.UniqueProjectKey + "?date=" + now.ToString());
        }

        private Uri GetIndividualDraftConfirmationUrl()
        {
            var now = DateTime.Now.ToOriginalTimeZone();
            if (Policy.AdvancedOptions.NoIndividualDraft)
                return null;

            return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/confirmIndividualDraft/" + Policy.GeneratedProperties.ProjectKey + Policy.GeneratedProperties.UniqueProjectKey + "?date=" + now.ToString());
        }

        private Uri GetResendIndividualDraftUrl()
        {
            var now = DateTime.Now.ToOriginalTimeZone();
            if (Policy.AdvancedOptions.NoIndividualDraft)
                return null;

            return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/sendIndividualDraft/" + Policy.GeneratedProperties.ProjectKey + Policy.GeneratedProperties.UniqueProjectKey + "?date=" + now.ToString());
        }

        private static DateTime GetDateTimeFromString(string date)
        {
            if (date == null)
                return new DateTime();

            return DateTime.Parse(date);
        }

        private Month GetCurrentOverride()
        {
            if (Policy.MonthlyOptions == null)
                return null;

            return Policy.MonthlyOptions.Find(o => o.MonthName.ToLower() == DateTime.Now.ToOriginalTimeZone().CurrentMonth().ToLower());
        }

        private bool IsThisMonthOverriden()
        {
            if (Policy.MonthlyOptions == null)
                return false;

            return Policy.MonthlyOptions.Exists(o => o.MonthName.ToLower() == DateTime.Now.ToOriginalTimeZone().CurrentMonth().ToLower());
        }

        private IDictionary<string, List<string>> GetUsersDictionary()
        {
            return Policy.UserOptions.ToDictionary(d => d.JiraUserKey, d => d.SourceControlUsernames);
        }

        public List<string> GetDraftAddedEmails()
        {
            return Policy.DraftEmails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public List<string> GetFinalAddedEmails()
        {
            return Policy.Emails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public void SetIndividualEmail(string emailAdress)
        {
            Policy.EmailCollection = new List<string>();
            Policy.EmailCollection.Add(emailAdress);
        }

        public static Policy LoadFromFile(string filePath)
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
                ser.Serialize(fs, Policy);
            }
        }



        public void SetDefaultProperties(Options options)
        {
            SetReportTitle();
            SetRootPath();
            SetPermanentTaskLabel();
            ResetToDefault();
            SetDraftMode(options);
        }

        public void ResetToDefault()
        {
            if (Policy.GeneratedProperties.LastDraftSentDate.Date == DateTime.Today || Policy.GeneratedProperties.LastDraftSentDate == new DateTime() || Policy.GeneratedProperties.WasResetToDefaultToday)
                return;

            ResetPolicyToDefault();
            Policy.GeneratedProperties.WasResetToDefaultToday = true;
        }

        private void SetDraftMode(Options options)
        {
            if (Policy.AdvancedOptions.NoDraft || Policy.GeneratedProperties.IsFinalDraftConfirmed)
                SetFinalReportMode();
            else
                SetFinalAndIndividualDrafts(options);
        }

        private void SetFinalAndIndividualDrafts(Options options)
        {
            if (this.CanSendFullDraft(options.TriggerKey))
            {
                Policy.GeneratedProperties.IsFinalDraft = true;
                Policy.GeneratedProperties.IsIndividualDraft = false;
            }
            else
            {
                Policy.GeneratedProperties.IsFinalDraft = false;
                Policy.GeneratedProperties.IsIndividualDraft = true;
            }
        }

        private void SetFinalReportMode()
        {
            Policy.GeneratedProperties.IsFinalDraft = false;
            Policy.GeneratedProperties.IsIndividualDraft = false;
            Policy.GeneratedProperties.IsFinalReport = true;
        }

        private void SetRootPath()
        {
            if (Policy.GeneratedProperties.RootPath == null)
                Policy.GeneratedProperties.RootPath = Path.GetFullPath(Policy.GeneratedProperties.ProjectName);
        }

        private void SetReportTitle()
        {
            if (Policy.AdvancedOptions.ReportTitle == null)
                Policy.AdvancedOptions.ReportTitle = Policy.GeneratedProperties.ProjectName + " Daily Report";
        }

        private void SetPermanentTaskLabel()
        {
            if (Policy.AdvancedOptions.PermanentTaskLabel != null)
                Policy.AdvancedOptions.PermanentTaskLabel = Policy.AdvancedOptions.PermanentTaskLabel.ToLower();
        }

        public void ResetPolicyToDefault()
        {
            Policy.GeneratedProperties.IsFinalDraftConfirmed = false;
            Policy.GeneratedProperties.IndividualDrafts = null;
            Policy.GeneratedProperties.WasForcedByLead = false;
        }

        private List<DayOfWeek> GetWeekendDays()
        {
            var daysList = Policy.AdvancedOptions.WeekendDays.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var weekendDaysEnum = new List<DayOfWeek>();
            try
            {
                foreach (var day in daysList)
                {
                    var dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day);
                    weekendDaysEnum.Add(dayOfWeek);
                }
                return weekendDaysEnum;
            }
            catch (Exception)
            {
                return new List<DayOfWeek>(){
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday
                    };
            }
        }
    }
}
