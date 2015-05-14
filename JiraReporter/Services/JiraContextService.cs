using Equilobe.DailyReport.Models.Storage;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Interfaces;

namespace JiraReporter.Services
{
    public class JiraContextService
    {
        public IConfigurationService ConfigurationService { get; set; }

        JiraPolicy Policy { get { return Context.Policy; } }
        JiraReport Context { get; set; }

        public JiraContextService(JiraReport report)
        {
            Context = report;
        }

        public void SetPolicy()
        {
            SetDefaultProperties();

            SetUrls();
            Policy.CurrentOverride = GetCurrentOverride();
            Policy.IsThisMonthOverriden = IsThisMonthOverriden();
            Policy.Users = GetUsersDictionary();
            SetReportType();
            SetMonthlyNonWorkingDays();
        }

        private void SetMonthlyNonWorkingDays()
        {
            if (Policy.MonthlyOptions != null)
                foreach (var month in Policy.MonthlyOptions)
                    if (!string.IsNullOrEmpty(month.NonWorkingDays))
                        month.NonWorkingDaysList = MonthlyOptionsHelpers.GetNonWorkingDays(month);
        }

        private void SetUrls()
        {
            Context.SendReportUrl = GetDraftConfirmationUrl();
            Context.SendDraftUrl = GetResendDraftUrl();
            Context.IndividualDraftConfirmationUrl = GetIndividualDraftConfirmationUrl();
            Context.SendIndividualDraftUrl = GetResendIndividualDraftUrl();
        }

        private Uri GetDraftConfirmationUrl()
        {
            var date = Context.Options.ToDate;
            if (Policy.AdvancedOptions.NoDraft)
                return null;

            return new Uri(ConfigurationService.GetWebBaseUrl() + "/app/report/confirmDraft/" + Context.UniqueProjectKey + "?date=" + date.ToShortDateString());
        }

        private Uri GetResendDraftUrl()
        {
            var date = Context.Options.ToDate;
            if (Policy.AdvancedOptions.NoDraft)
                return null;

            return new Uri(ConfigurationService.GetWebBaseUrl() + "/app/report/sendDraft/" + Context.UniqueProjectKey + "?date=" + date.ToShortDateString());
        }

        private Uri GetIndividualDraftConfirmationUrl()
        {
            var date = Context.Options.ToDate;
            if (Policy.AdvancedOptions.NoIndividualDraft)
                return null;

            return new Uri(ConfigurationService.GetWebBaseUrl() + "/app/report/confirmIndividualDraft/" + Context.UniqueProjectKey + "?date=" + date.ToShortDateString());
        }

        private Uri GetResendIndividualDraftUrl()
        {
            var date = Context.Options.ToDate;
            if (Policy.AdvancedOptions.NoIndividualDraft)
                return null;

            return new Uri(ConfigurationService.GetWebBaseUrl() + "/app/report/sendIndividualDraft/" + Context.UniqueProjectKey + "?date=" + date.ToShortDateString());
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

            return Policy.MonthlyOptions.Find(o => o.MonthName.ToLower() == DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc).CurrentMonth().ToLower());
        }

        private bool IsThisMonthOverriden()
        {
            if (Policy.MonthlyOptions == null)
                return false;

            return Policy.MonthlyOptions.Exists(o => o.MonthName.ToLower() == DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc).CurrentMonth().ToLower());
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



        public void SetDefaultProperties()
        {
            if (Policy.AdvancedOptions == null)
                Policy.AdvancedOptions = new AdvancedOptions();
            if (string.IsNullOrEmpty(Policy.AdvancedOptions.ReportTitle))
                Policy.AdvancedOptions.ReportTitle = Context.ProjectName;
        }

        private void SetReportType()
        {
            if (Context.IsOnSchedule)
                SetScheduledReportType();
            else
                SetReportTypeBySendingScope();
        }

        private void SetReportTypeBySendingScope()
        {
            if (Context.ExecutionInstance.Scope == SendScope.SendFinalDraft)
                Context.IsFinalDraft = true;
            else
                if (Context.ExecutionInstance.Scope == SendScope.SendIndividualDraft)
                    Context.IsIndividualDraft = true;
                else
                    if (Context.ExecutionInstance.Scope == SendScope.SendReport)
                        Context.IsFinalReport = true;
        }

        private void SetScheduledReportType()
        {
            if (Context.Policy.AdvancedOptions.NoDraft)
                SetFinalReportMode();
            else
                if (Context.Policy.AdvancedOptions.NoIndividualDraft && !Context.Policy.AdvancedOptions.NoDraft)
                    SetFullDraftMode();
                else
                    SetIndividualDraftMode();
        }

        private void SetIndividualDraftMode()
        {
            Context.IsFinalDraft = false;
            Context.IsIndividualDraft = true;
        }

        private void SetFullDraftMode()
        {
            Context.IsFinalDraft = true;
            Context.IsIndividualDraft = false;
        }

        private void SetFinalReportMode()
        {
            Context.IsFinalDraft = false;
            Context.IsIndividualDraft = false;
            Context.IsFinalReport = true;
        }
    }
}
