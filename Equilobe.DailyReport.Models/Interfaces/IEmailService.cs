using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IEmailService : IService
    {
        void SendEmail(MailMessage message);

        MailMessage GetHtmlMessage(List<string> recipients, string subject, string body);

        MailMessage GetHtmlMessage(string recipient, string subject, string body);
    }
}
