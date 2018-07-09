using Equilobe.DailyReport.JWT;
using Equilobe.DailyReport.Utils;
using RestSharp;
using System;
using System.Net;

namespace Equilobe.DailyReport.BL
{
    public static class RestApiHelper
    {
        public static RestClient BasicAuthentication(string baseUrl, string username, string password)
        {
            return new RestClient(baseUrl)
            {
                Authenticator = new HttpBasicAuthenticator(username, password)
            };
        }

        public static RestClient JwtAuthentication(string baseUrl, string sharedSecret, string addonKey)
        {
            return new RestClient(baseUrl)
            {
                Authenticator = new JwtAuthenticator(sharedSecret, addonKey)
            };
        }

        public static T ResolveRequest<T>(this RestClient client, RestRequest request, bool isXml = false)
        {
            var response = client.Execute(request);

            ValidateResponse(response);

            if (isXml)
                return Deserialization.XmlDeserialize<T>(response.Content);

            return Deserialization.JsonDeserialize<T>(response.Content);
        }


        public static T ResolveJiraRequest<T>(this RestClient client, RestRequest request) where T : new()
        {
            var response = client.Execute<T>(request);

            ValidateResponse(response);

            return response.Data;
        }

        public static void ValidateResponse(IRestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NoContent ||
                response.ErrorException != null ||
                response.ResponseStatus != ResponseStatus.Completed)
                throw new InvalidOperationException(string.Format("RestSharp status: {0}, HTTP response: {1}", response.ResponseStatus, !String.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage : response.StatusDescription));
        }
    }
}
