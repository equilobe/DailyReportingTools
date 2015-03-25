﻿using Equilobe.DailyReport.BL.Jira;
using Equilobe.DailyReport.JWT;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Configuration;
using System.Net;
using System.Text;

namespace JiraReporter.Helpers
{
    public static class AuthorizationExtensions
    {
        public static void Authorize(this WebClient client, JiraRequestContext context, string relativeUrl)
        {
            if (!string.IsNullOrEmpty(context.SharedSecret))
                client.Headers.Add("Authorization", "JWT " + JwtAuthenticator.CreateJwt(ConfigurationManager.AppSettings["addonKey"], context.SharedSecret, relativeUrl, "GET"));
            else
                client.Headers.Add("Authorization", "Basic " + CreateBasic(context.JiraUsername, context.JiraPassword));
        }

        static string CreateBasic(string username, string password)
        {
            var byteString = UTF8Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password));
            return Convert.ToBase64String(byteString);
        }
    }    
}
