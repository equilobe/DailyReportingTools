using JWT;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Equilobe.DailyReport.JWT
{
    public class JwtAuthenticator : IAuthenticator
    {
        private string _sharedSecret;
        private string _addonKey;

        public JwtAuthenticator(string sharedSecret, string addonKey)
        {
            _sharedSecret = sharedSecret;
            _addonKey = addonKey;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            var jwtToken = CreateJwt(_addonKey, _sharedSecret, request.Resource, request.Method.ToString());
            request.AddParameter("Authorization", "JWT " + jwtToken, ParameterType.HttpHeader);
        }

        public static string CreateJwt(string addonKey, string sharedSecret, string relativeUrl, string method)
        {
            var canonicalUrl = GenerateCanonicalRequest(relativeUrl, method);
            var qsh = CalculateHash(canonicalUrl);

            var tokenBuilder = new DecodedJwtToken(sharedSecret);
            tokenBuilder.Claims.Add("qsh", qsh);
            tokenBuilder.Claims.Add("iat", DateTime.UtcNow.AsUnixTimestampSeconds());
            tokenBuilder.Claims.Add("exp", DateTime.UtcNow.AddMinutes(5).AsUnixTimestampSeconds());
            tokenBuilder.Claims.Add("iss", addonKey);

            return tokenBuilder.Encode().JwtTokenString;
        }

        public static string GenerateCanonicalRequest(string relativeUrl, string method)
        {
            return new StringBuilder()
                .Append(method)
                .Append("&")
                .Append(GetPath(relativeUrl))
                .Append("&")
                .Append(GetCanonicalQueryString(relativeUrl))
                .ToString();
        }

        public static string CalculateHash(string relativeUrl, string method)
        {
            return CalculateHash(GenerateCanonicalRequest(relativeUrl, method));
        }

        public static string CalculateHash(string canonicalRequest)
        {
            using (var sha = SHA256.Create())
            {
                var computedHash = sha.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest));
                return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
            }
        }

        private static string GetCanonicalQueryString(string relativeUrl)
        {
            var queryString = GetQueryString(relativeUrl);
            var queryStringItems = HttpUtility.ParseQueryString(queryString);

            return String.Join("&", queryStringItems
                .AllKeys
                .Where(x => x != "jwt")
                .Select(x => new KeyValuePair<String, String[]>(x, queryStringItems.GetValues(x)))
                .OrderBy(x => x.Key)
                .Select(x =>
                {
                    return String.Format("{0}={1}", x.Key,
                        String.Join(",", x.Value
                            .OrderBy(i => i)
                            .Select(EscapeUriDataStringRfc3986)));
                }));
        }

        private static string GetQueryString(string relativeUrl)
        {
            return relativeUrl.IndexOf('?') != -1 ? relativeUrl.Substring(relativeUrl.IndexOf('?') + 1) : String.Empty;
        }

        private static string GetPath(string relativeUrl)
        {
            var path = relativeUrl.Split('?')[0];
            if (!path.StartsWith("/"))
                path = "/" + path;
            return path;
        }

        private static readonly char[] UriRfc3986CharsToEscape = new[] { '!', '*', '\'', '(', ')' };
        internal static string EscapeUriDataStringRfc3986(string value)
        {
            StringBuilder escaped = new StringBuilder(Uri.EscapeDataString(value));
            foreach (char charToEscape in UriRfc3986CharsToEscape)
                escaped.Replace(charToEscape.ToString(), Uri.HexEscape(charToEscape));

            return escaped.ToString();
        }
    }
}
