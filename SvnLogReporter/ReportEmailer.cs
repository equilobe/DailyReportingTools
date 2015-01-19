using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Mail;
using System.Globalization;
using System.Diagnostics;
using SourceControlLogReporter.Model;

namespace SourceControlLogReporter
{
    public class ReportEmailer
    {
        public Policy policy;
        public Options options;

        public ReportEmailer(Policy p, Options o)
        {
            this.policy = p;
            this.options = o;
        }


        private void SendEmails()
        {
            if (options.NoEmail)
                return;

            Validation.EnsureDirectoryExists(policy.GeneratedProperties.UnsentReportsPath);

            foreach (var file in Directory.GetFiles(policy.GeneratedProperties.UnsentReportsPath))
            {
                TryEmailReport(file);
            }
        }

        public void TrySendEmails()
        {
            try
            {
                SendEmails();
            }
            catch
            {
            }
        }

        public void TryEmailReport(string path)
        {
            try
            {
                EmailReport(path);
            }
            catch (Exception)
            {
                Debug.WriteLine("Email not available");

            }
        }

        public virtual void EmailReport(string reportPath)
        {
            EmailReportMessage(GetMessage(reportPath), reportPath);
        }

        protected void EmailReportMessage(MailMessage message, string reportPath)
        {
            var smtp = new SmtpClient { EnableSsl = true };
            smtp.Send(message);
            MoveToSent(reportPath);
            UpdateLastReportSentDate();
        }

        protected void UpdateLastReportSentDate()
        {
            if (!policy.AdvancedOptions.NoDraft)
                return;

            policy.GeneratedProperties.LastReportSentDate = options.ToDate;
            policy.SaveToFile(options.PolicyPath);
        }

        public virtual MailMessage GetMessage(string reportPath)
        {
            var message = new MailMessage
            {
                Subject = GetReportSubject(reportPath),
                Body = File.ReadAllText(reportPath),
                IsBodyHtml = true
            };
            foreach (string addr in policy.EmailCollection)
                message.To.Add(addr);
            return message;
        }

        public virtual string GetReportSubject(string reportPath)
        {
            string subject = string.Empty;
            if (!policy.AdvancedOptions.NoDraft)
                subject += "DRAFT ";
            subject += policy.AdvancedOptions.ReportTitle + " for ";
            if ((options.ToDate - options.FromDate).Days > 1)
                return subject + " " + options.FromDate.ToString("ddd, dd MMM") + " - " + options.ToDate.AddDays(-1).ToString("ddd, dd MMM");
            else
                return subject + " " + options.FromDate.ToString("ddd, dd MMM yyyy");
        }

        public void MoveToSent(string path)
        {
            Validation.EnsureDirectoryExists(policy.GeneratedProperties.ReportsPath);

            var newFilePath = Path.Combine(policy.GeneratedProperties.ReportsPath, Path.GetFileName(path));

            File.Copy(path, newFilePath, overwrite: true);
            File.Delete(path);
        }
    }
}
