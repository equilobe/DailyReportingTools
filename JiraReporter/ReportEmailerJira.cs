﻿using System;
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
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.ReportFrame;
using JiraReporter.Services;
using Equilobe.DailyReport.Utils;

namespace JiraReporter
{
    public class ReportEmailerJira : ReportEmailer
    {
        public List<JiraAuthor> Authors { get { return Report.Authors; } }
        public JiraAuthor Author { get { return Report.Author; } }
        public JiraPolicy Policy { get { return Report.Policy; } }
        public JiraOptions Options { get { return Report.Options; } }
        public JiraReport Report { get; set; }

        public ReportEmailerJira(JiraReport report)
        {
            Report = report;
        }
        public void AddAttachementImage(Image image, string id, MailMessage mailMessage)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            var imgBytes = stream.ToArray();

            MemoryStream ms = new MemoryStream(imgBytes);
            var attachment = new Attachment(ms, id, "image/png");
            AddAttachmentToMailMessage(mailMessage, attachment, id);
        }

        public void AddAttachementImageFromDisk(MailMessage mailMessage)
        {
            string attachmentRootPath = Environment.CurrentDirectory + @"\Content\Images";
            foreach(var file in Directory.GetFiles(attachmentRootPath))
            {
                var image = Image.FromFile(file);             
                var attachmentId = Path.GetFileName(file);
                AddAttachementImage(image, attachmentId, mailMessage);
            }
        }

        public void AddAttachmentToMailMessage(MailMessage message, Attachment attachment, string attachmentId)
        {
            attachment.ContentDisposition.Inline = true;
            attachment.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
            attachment.ContentId = attachmentId;
            attachment.ContentType.Name = attachmentId;
            message.Attachments.Add(attachment);
        }

        public override MailMessage GetMessage(string reportPath)
        {
            var message = new MailMessage
            {
                Subject = GetReportSubject(reportPath),
                Body = File.ReadAllText(reportPath),
                IsBodyHtml = true
            };

            AddMailAttachments(message);

            AddMailRecipients(message);

            return message;
        }

        private void AddMailRecipients(MailMessage message)
        {
            foreach (string addr in Policy.EmailCollection)
                message.To.Add(addr);
        }

        private void AddMailAttachments(MailMessage message)
        {
            if (Policy.GeneratedProperties.IsIndividualDraft)
                AddAttachementImage(Author.Image, Author.AvatarId, message);
            else
            {
                foreach (var author in Authors)
                    AddAttachementImage(author.Image, author.AvatarId, message);
                AddAttachementImageFromDisk(message);
            }
        }

        public override string GetReportSubject(string reportPath)
        {
            string subject = string.Empty;

            if (Policy.GeneratedProperties.IsFinalDraft || Policy.GeneratedProperties.IsIndividualDraft)
                subject += "DRAFT | ";
            if (Policy.GeneratedProperties.IsIndividualDraft)
                subject += Author.Name + " | ";
            subject += Policy.AdvancedOptions.ReportTitle + " | ";
            subject += ReportDateFormatter.GetReportDate(Options.FromDate, Options.ToDate);

            return subject;
        }

        protected override void SendEmails()
        {
            Validation.EnsureDirectoryExists(Policy.GeneratedProperties.UnsentReportsPath);

            foreach (var file in Directory.GetFiles(Policy.GeneratedProperties.UnsentReportsPath))
            {
                TryEmailReport(file);
            }
        }

        public override void MoveToSent(string path)
        {
            Validation.EnsureDirectoryExists(Policy.GeneratedProperties.ReportsPath);

            var newFilePath = Path.Combine(Policy.GeneratedProperties.ReportsPath, Path.GetFileName(path));

            File.Copy(path, newFilePath, overwrite: true);
            File.Delete(path);
        }

        protected override void UpdatePolicy()
        {
            var policyService = new JiraPolicyService(Report);
            if (Policy.GeneratedProperties.IsFinalDraft)
            {
                Policy.GeneratedProperties.IsFinalDraftConfirmed = false;
                Policy.GeneratedProperties.LastDraftSentDate = Options.ToDate;
                if (Policy.IsForcedByLead(Options.TriggerKey))
                    Policy.GeneratedProperties.WasForcedByLead = true;
            }

            if (Policy.GeneratedProperties.IsFinalReport)
            {
                Policy.GeneratedProperties.LastReportSentDate = Options.ToDate;
                policyService.ResetPolicyToDefault();
                Policy.GeneratedProperties.WasResetToDefaultToday = false;
            }

            JiraPolicyService.SaveToFile(Options.PolicyPath, Policy);
        }
    }
}
