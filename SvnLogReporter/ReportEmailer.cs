﻿using System;
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

            Validation.EnsureDirectoryExists(policy.UnsentReportsPath);

            foreach (var file in Directory.GetFiles(policy.UnsentReportsPath))
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

            policy.LastReportSentDate = options.ToDate;
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

        public string GetReportSubject(string reportPath)
        {
            string subject = policy.ReportTitle + " " + policy.EmailSubject;
            if ((options.ToDate - options.FromDate).Days > 1)
                return subject + " " + options.FromDate.ToString("dddd, dd MMMM") + " - " + options.ToDate.AddDays(-1).ToString("dddd, dd MMMM");
            else
                return subject + " " + options.FromDate.ToString("dddd, dd MMMM yyyy");
        }

        public void MoveToSent(string path)
        {
            Validation.EnsureDirectoryExists(policy.ReportsPath);

            var newFilePath = Path.Combine(policy.ReportsPath, Path.GetFileName(path));

            File.Copy(path, newFilePath, overwrite: true);
            File.Delete(path);
        }
    }
}
