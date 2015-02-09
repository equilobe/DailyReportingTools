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
using Equilobe.DailyReport.Models.ReportPolicy;
using Equilobe.DailyReport.Models.Jira;
using JiraReporter.Services;

namespace JiraReporter
{
    public class ReportEmailerJira : ReportEmailer
    {
        public List<JiraAuthor> Authors { get; set; }
        public JiraAuthor Author { get; set; }
        public JiraPolicy Policy { get; set; }
        public JiraOptions Options { get; set; }

        public ReportEmailerJira(JiraPolicy policy, JiraOptions options)
        {
            Policy = policy;
            Options = options;
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
                AddAttachementImage(Author.Image, Author.Username, message);
            else
                foreach (var author in Authors)
                    AddAttachementImage(author.Image, author.Username, message);
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

        protected override void UpdatePolicy()
        {
            var policyService = new JiraPolicyService(Policy);
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
