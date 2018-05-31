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
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.SourceControl;
using Equilobe.DailyReport.Utils;
using Equilobe.DailyReport.Models.Policy;

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

        public ReportEmailer()
        {

        }

        public virtual void SendEmails()
        {
            if (options.NoEmail)
                return;

            Validations.EnsureDirectoryExists(policy.UnsentReportsPath);

            foreach (var file in Directory.GetFiles(policy.UnsentReportsPath))
            {
                EmailReport(file);
            }
        }

        public virtual void EmailReport(string reportPath)
        {
            EmailReportMessage(GetMessage(reportPath), reportPath);
        }

        protected void EmailReportMessage(MailMessage message, string reportPath)
        {
            var smtp = new SmtpClient();
            smtp.Send(message);
            MoveToSent(reportPath);
        }

        public virtual MailMessage GetMessage(string reportPath)
        {
            var message = new MailMessage
            {
                Subject = GetReportSubject(reportPath),
                Body = File.ReadAllText(reportPath),
                IsBodyHtml = true
            };

            //foreach (string addr in policy.EmailCollection)
            //{
            //    if (!Validations.Mail(addr))
            //        continue;

            //    message.To.Add(addr);
            //}

            message.To.Add("georgian.tanase@equilobe.com");

            return message;
        }

        public virtual string GetReportSubject(string reportPath)
        {
            string subject = string.Empty;
            subject += policy.ReportTitle + " | ";
            subject += ReportDateFormatter.GetReportDate(options.FromDate, options.ToDate);

            return subject;
        }

        public virtual void MoveToSent(string path)
        {
            Validations.EnsureDirectoryExists(policy.ReportsPath);

            var newFilePath = Path.Combine(policy.ReportsPath, Path.GetFileName(path));

            File.Copy(path, newFilePath, overwrite: true);
            File.Delete(path);
        }
    }
}
