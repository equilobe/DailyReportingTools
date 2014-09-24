﻿using AnotherJiraRestClient;
using JiraReporter.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class RestApiRequests
    {
        public static Timesheet GetTimesheet(SvnLogReporter.Model.Policy policy, DateTime startDate, DateTime endDate)
        {
            string fromDate = TimeFormatting.DateToString(startDate);
            string toDate = TimeFormatting.DateToString(endDate);
            var client = new RestClient(policy.BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(policy.Username, policy.Password);
            var request = new RestRequest(ApiUrls.Timesheet(policy.TargetGroup, fromDate, toDate), Method.GET);
            var response = client.Execute(request);
            string xmlString = response.Content;
            return Deserialization.XmlDeserialize<Timesheet>(xmlString);
        }

        public static List<User> GetUsers(SvnLogReporter.Model.Policy policy)
        {
            var client = new RestClient(policy.BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(policy.Username, policy.Password);
            var request = new RestRequest(ApiUrls.Users(policy.Project), Method.GET);
            var response = client.Execute(request);
            string contentString = response.Content;
            return Deserialization.JsonDeserialize<List<User>>(contentString);
        }

        public static AnotherJiraRestClient.Issues GetCompletedIssues(SvnLogReporter.Model.Policy policy, DateTime startDate, DateTime endDate)
        {
            string fromDate = TimeFormatting.DateToISO(startDate);
            string toDate = TimeFormatting.DateToISO(endDate);
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var issues = client.GetIssuesByJql(ApiUrls.ResolvedIssues(fromDate, toDate), 0, 250);
            return issues;
        }

        public static AnotherJiraRestClient.Issues GetSprintTasks(SvnLogReporter.Model.Policy policy)
        {
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var tasks = client.GetIssuesByJql(ApiUrls.IssuesInOpenSprints(policy.Project), 0, 250);
            return tasks;
        }

        public static AnotherJiraRestClient.Issue GetIssue(string issueKey, SvnLogReporter.Model.Policy policy)
        {
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var issue = client.GetIssue(issueKey);
            return client.GetIssue(issueKey);
        }
    }
}
