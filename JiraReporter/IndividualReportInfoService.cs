﻿using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class IndividualReportInfoService
    {
        public void SetIndividualDraftInfo(List<Author> authors, SourceControlLogReporter.Model.Policy policy)
        {
            if(!policy.AdvancedOptions.NoIndividualDraft && policy.GeneratedProperties.IsIndividualDraft)
            {
                var individualDrafts = new List<SourceControlLogReporter.Model.IndividualDraftInfo>();
                var random = new Random();

                foreach(var author in authors)
                {
                    var individualDraft = GenerateIndividualDraftInfo(author, policy, random);
                    individualDrafts.Add(individualDraft);
                    author.IndividualDraftInfo = individualDraft;
                }

                policy.GeneratedProperties.IndividualDrafts = individualDrafts;
            }
        }

        private SourceControlLogReporter.Model.IndividualDraftInfo GenerateIndividualDraftInfo(Author author, SourceControlLogReporter.Model.Policy policy, Random random)
        {
            var individualDraft = new SourceControlLogReporter.Model.IndividualDraftInfo
            {
                Name = author.Name,
                Username = author.Username,
                UserKey = RandomGenerator.RandomString(10, random),
                IsLead = author.IsProjectLead
            };
            SetIndividualUrls(individualDraft, policy);

            return individualDraft;
        }

        private void SetIndividualUrls(SourceControlLogReporter.Model.IndividualDraftInfo individualDraft, SourceControlLogReporter.Model.Policy policy)
        {
            individualDraft.ConfirmationDraftUrl = GetUrl(individualDraft, policy.IndividualDraftConfirmationUrl);
            individualDraft.ResendDraftUrl = GetUrl(individualDraft, policy.ResendIndividualDraft);
            if (individualDraft.IsLead)
                individualDraft.ForceDraftUrl = GetUrl(individualDraft, policy.ResendDraftUrl);
        }

        private static Uri GetUrl(SourceControlLogReporter.Model.IndividualDraftInfo individualDraft, Uri baseUrl)
        {
            var url = string.Format("draftKey={0}", individualDraft.UserKey);

            return new Uri(baseUrl + "&" + url);
        }

        public SourceControlLogReporter.Model.IndividualDraftInfo GetIndividualDraftInfo(string key, SourceControlLogReporter.Model.Policy policy)
        {
            var draft = new SourceControlLogReporter.Model.IndividualDraftInfo();
            if (policy.GeneratedProperties.IndividualDrafts != null)
                draft = policy.GeneratedProperties.IndividualDrafts.Find(d => d.UserKey == key);
            SetIndividualUrls(draft, policy);

            return draft;
        }
    }
}