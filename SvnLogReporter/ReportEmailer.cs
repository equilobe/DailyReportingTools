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
using SvnLogReporter.Model;

namespace SvnLogReporter
{
    class ReportEmailer
    {
        Policy policy;
        Options options;
        
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

        void TryEmailReport(string path)
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

        void EmailReport(string reportPath)
        {
            var smtp = new SmtpClient { EnableSsl = true };
            smtp.Send(GetMessage(reportPath));

            MoveToSent(reportPath);
        }

        private MailMessage GetMessage(string reportPath)
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

        private string GetReportSubject(string reportPath)
        {
            if (options.ToDate.Day - options.FromDate.Day > 1)
                return policy.ReportTitle + " " + policy.ReportType + "Report for " + options.FromDate.ToString("dddd, dd MMMM yyyy") + " - " + options.ToDate.AddDays(-1).ToString("dddd, dd MMMM yyyy");
            else
                return policy.ReportTitle + " " + policy.ReportType + " Report for " + DateTime.Parse(Path.GetFileNameWithoutExtension(reportPath)).ToString("dddd, dd MMMM yyyy");

        }

        private void MoveToSent(string path)
        {
            Validation.EnsureDirectoryExists(policy.ReportsPath);

            var newFilePath = Path.Combine(policy.ReportsPath, Path.GetFileName(path));
            
            File.Copy(path, newFilePath, overwrite: true);
            File.Delete(path);
        }
    }
}
