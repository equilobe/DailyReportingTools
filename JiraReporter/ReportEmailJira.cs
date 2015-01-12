using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourceControlLogReporter;
using System.Drawing;
using System.Net.Mail;
using System.IO;
using System.Drawing.Imaging;
using JiraReporter.Model;
using System.Net.Mime;

namespace JiraReporter
{
    public class ReportEmailJira : ReportEmailer
    {
        public List<Author> Authors { get; set; }

        public ReportEmailJira(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
            : base(policy, options)
        {
            this.policy = policy;
            this.options = options;
        }
        public void AddAttachementImage(Image image, string id, MailMessage mailMessage)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            var imgBytes = stream.ToArray();

            MemoryStream ms = new MemoryStream(imgBytes);
            var attachment = new Attachment(ms, id, "image/png");
            attachment.ContentDisposition.Inline = true;
            attachment.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
            attachment.ContentId = id;
            attachment.ContentType.Name = id;
            mailMessage.Attachments.Add(attachment);
        }

        public override void EmailReport(string reportPath)
        {
            var message = GetMessage(reportPath);
            if (Authors != null)
                foreach (var author in Authors)
                    AddAttachementImage(author.Image, author.Username, message);

            EmailReportMessage(message, reportPath);
        }
    }
}
