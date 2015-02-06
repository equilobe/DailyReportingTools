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
    public class JiraPolicyService
    {
        JiraPolicy Policy { get; set; }

        public JiraPolicyService(JiraPolicy policy)
        {
            Policy = policy;
        }

        public void SetPolicy(JiraOptions options)
        {
            SetDefaultProperties(options);

            SetUrls();
            Policy.CurrentOverride = GetCurrentOverride();
            Policy.IsThisMonthOverriden = IsThisMonthOverriden();
            Policy.Users = GetUsersDictionary();
            Policy.AdvancedOptions.WeekendDaysList = GetWeekendDays();
            Policy.ReportTimeDateFormat = GetDateTimeFromString(Policy.ReportTime);
            SetMonthlyNonWorkingDays();
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

        public static List<string> GetDraftAddedEmails(JiraPolicy policy)
        {
            return policy.DraftEmails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static List<string> GetFinalAddedEmails(JiraPolicy policy)
        {
            return policy.Emails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static void SetIndividualEmail(string emailAdress, JiraPolicy policy)
        {
            policy.EmailCollection = new List<string>();
            policy.EmailCollection.Add(emailAdress);
        }

        public static JiraPolicy LoadFromFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                XmlSerializer ser = new XmlSerializer(typeof(JiraPolicy));
                return (JiraPolicy)ser.Deserialize(fs);
            }
        }

        public static void SaveToFile(string filePath, JiraPolicy policy)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                XmlSerializer ser = new XmlSerializer(typeof(JiraPolicy));
                ser.Serialize(fs, policy);
            }
        }



        public void SetDefaultProperties(JiraOptions options)
        {
            Policy.AdvancedOptions = new JiraAdvancedOptions();
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

        private void SetDraftMode(JiraOptions options)
        {
            if (Policy.AdvancedOptions.NoDraft || Policy.GeneratedProperties.IsFinalDraftConfirmed)
                SetFinalReportMode();
            else
                SetFinalAndIndividualDrafts(options);
        }

        private void SetFinalAndIndividualDrafts(JiraOptions options)
        {
            if (Policy.CanSendFullDraft(options.TriggerKey))
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

        public static void SetPolicyFinalReport(JiraPolicy policy, string policyPath)
        {
            policy.GeneratedProperties.IsFinalDraftConfirmed = true;
            SaveToFile(policyPath, policy);
        }

        public static string GetPolicyPath(string key)
        {
            var basePath = ConfigurationManager.AppSettings["JiraReporterPath"];
            var policyPath = basePath + @"\Policies\" + key + ".xml";
            return policyPath;
        }

        public static JiraPolicy LoadPolicy(string id)
        {
            var policyPath = GetPolicyPath(id);
            return LoadFromFile(policyPath);
        }

        public static bool SetIndividualDraftConfirmation(JiraPolicy policy, string key, string policyPath)
        {
            try
            {
                var draftsInfo = policy.GeneratedProperties.IndividualDrafts;
                var draft = draftsInfo.Find(d => d.UserKey == key);
                draft.Confirmed = true;
                SaveToFile(policyPath, policy);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
