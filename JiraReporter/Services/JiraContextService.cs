using Equilobe.DailyReport.Models.Storage;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Policy;

namespace JiraReporter.Services
{
    public class JiraContextService
    {
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
            Policy.AdvancedOptions.WeekendDaysList = GetWeekendDays();
            Policy.ReportTimeDateFormat = GetDateTimeFromString(Policy.ReportTime);
            SetReportType();
            SetMonthlyNonWorkingDays();
        }

        private void SetMonthlyNonWorkingDays()
        {
            if (Policy.MonthlyOptions != null)
                foreach (var month in Policy.MonthlyOptions.Months)
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
            var now = DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc);
            if (Policy.AdvancedOptions.NoDraft)
                return null;

            return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/sendReport/" + Context.UniqueProjectKey + "?date=" + now.ToString());
        }

        private Uri GetResendDraftUrl()
        {
            var now = DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc);
            if (Policy.AdvancedOptions.NoDraft)
                return null;

            return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/sendDraft/" + Context.UniqueProjectKey + "?date=" + now.ToString());
        }

        private Uri GetIndividualDraftConfirmationUrl()
        {
            var now = DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc);
            if (Policy.AdvancedOptions.NoIndividualDraft)
                return null;

            return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/confirmIndividualDraft/" + Context.UniqueProjectKey + "?date=" + now.ToString());
        }

        private Uri GetResendIndividualDraftUrl()
        {
            var now = DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc);
            if (Policy.AdvancedOptions.NoIndividualDraft)
                return null;

            return new Uri(ConfigurationManager.AppSettings["webBaseUrl"] + "/report/sendIndividualDraft/" + Context.UniqueProjectKey + "?date=" + now.ToString());
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

            return Policy.MonthlyOptions.Months.Find(o => o.MonthName.ToLower() == DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc).CurrentMonth().ToLower());
        }

        private bool IsThisMonthOverriden()
        {
            if (Policy.MonthlyOptions == null)
                return false;

            return Policy.MonthlyOptions.Months.Exists(o => o.MonthName.ToLower() == DateTime.Now.ToOriginalTimeZone(Context.OffsetFromUtc).CurrentMonth().ToLower());
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
