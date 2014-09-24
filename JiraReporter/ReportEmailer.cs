using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class ReportEmailer
    {
        SvnLogReporter.Model.Policy policy;
        SvnLogReporter.Options options;

        public ReportEmailer(SvnLogReporter.Model.Policy p, SvnLogReporter.Options o)
        {
            this.policy = p;
            this.options = o;
        }

         public void EmailReport(string reportPath)
        {
            var smtp = new SmtpClient { EnableSsl = true };
            smtp.Send(GetMessage(reportPath));
        }

        private MailMessage GetMessage(string reportPath)
        {
            var message = new MailMessage
            {
                Subject = GetEmailSubject(),
                Body = File.ReadAllText(reportPath),
                IsBodyHtml = true
            };
            foreach (string addr in policy.EmailCollection)
                message.To.Add(addr);
            return message;
        }

        private string GetEmailSubject()
        {
            if ((options.ToDate - options.FromDate).Days <= 1)
                return policy.ReportTitle + " Daily Report for " + options.FromDate.ToString("dddd, dd MMMM yyyy");
            else
                return policy.ReportTitle + " Daily Report for " + options.FromDate.ToString("dddd, dd MMMM yyyy") + " - " + options.ToDate.AddDays(-1).ToString("dddd, dd MMMM yyyy");
        }
    }
}
