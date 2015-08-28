using Equilobe.DailyReport.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.SL
{
    public class EmailService : IEmailService
    {
        public void SendEmail(MailMessage message)
        {
            var smtpCLient = new SmtpClient();

            smtpCLient.Send(message);
        }

        public MailMessage GetHtmlMessage(List<string> recipients, string subject, string body)
        {
            var message = new MailMessage
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            foreach (var adress in recipients)
                message.To.Add(adress);

            return message;
        }

        public MailMessage GetHtmlMessage(string recipient, string subject, string body)
        {
            var message = new MailMessage
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(recipient);

            return message;
        }
    }
}
