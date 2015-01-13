using JWT;
using RestSharp;
using RestSharp.Contrib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;

namespace JiraReporter
{
    public class JwtAuthenticator : IAuthenticator
    {
        private string _sharedSecret;

        public JwtAuthenticator(string sharedSecret)
        {
            _sharedSecret = sharedSecret;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {            
            var jwtToken = CreateJwt(ConfigurationManager.AppSettings["addonKey"], _sharedSecret, request.Resource, request.Method.ToString());
            request.AddParameter("jwt", jwtToken, ParameterType.UrlSegment);
        }

        public static string CreateJwt(string addonKey, string sharedSecret, string query, string method)
        {
            var tokenBuilder = new DecodedJwtToken(sharedSecret);
            
            string hashedQshClaim = GetHashedQshClaim(query, method);
            tokenBuilder.Claims.Add("qsh", hashedQshClaim);

            tokenBuilder.Claims.Add("iat", DateTime.UtcNow.AsUnixTimestampSeconds());
            tokenBuilder.Claims.Add("exp", DateTime.UtcNow.AddMinutes(5).AsUnixTimestampSeconds());
            tokenBuilder.Claims.Add("iss", addonKey);

            return tokenBuilder.Encode().JwtTokenString;
        }

        private static string GetHashedQshClaim(string relativeUrl, string method)
        {
            var unhashedQshClaim = GetUnhasedQshClaim(relativeUrl, method);
            return HashQshClaim(unhashedQshClaim);
        }

        private static string HashQshClaim(string unhashedQshClaim)
        {
            using (var sha = SHA256.Create())
            {
                var computedHash = sha.ComputeHash(Encoding.UTF8.GetBytes(unhashedQshClaim));
                return BitConverter.ToString(computedHash)
                                             .Replace("-", "")
                                             .ToLower();
            }
        }

        private static string GetUnhasedQshClaim(string relativeUrl, string method)
        {
            var queryStringClaim = GetQueryStringClaim(relativeUrl);
            var path = GetPath(relativeUrl);
            return string.Format("{0}&{1}&{2}", method, path, queryStringClaim);
        }

        private static string GetQueryStringClaim(string relativeUrl)
        {
            var sortedQueryString = GetSortedQueryString(relativeUrl);            
            return new Regex(@"%[a-f0-9]{2}").Replace(sortedQueryString, m => m.Value.ToUpperInvariant());
        }

        private static string GetSortedQueryString(string relativeUrl)
        {
            var elements = HttpUtility.ParseQueryString(relativeUrl);
            var sortedKeys = elements.AllKeys
                        .OrderBy(x => x)
                        .ToList();

            return String.Join("&", sortedKeys
                    .Where(x => x != "jwt")
                    .Select(x => string.Format("{0}={1}", x, HttpUtility.UrlEncode(elements[x]))));
        }

        private static string GetPath(string relativeUrl)
        {
            var path = relativeUrl.Split('?')[0];
            if (!path.StartsWith("/"))
                path = "/" + path;
            return path;
        }
    }

    public class EncodedJwtToken
    {
        public EncodedJwtToken(string sharedSecret, string jwtToken)
        {
            SharedSecret = sharedSecret;
            JwtTokenString = jwtToken;
        }

        public string SharedSecret { get; set; }
        public string JwtTokenString { get; set; }
    }

    public class DecodedJwtToken
    {
        public DecodedJwtToken(string sharedSecret)
        {
            SharedSecret = sharedSecret;
            Claims = new Dictionary<string, object>();
        }

        public string SharedSecret { get; set; }
        public IDictionary<string, object> Claims { get; set; }

        public EncodedJwtToken Encode()
        {
            return new EncodedJwtToken(SharedSecret, JsonWebToken.Encode(Claims, SharedSecret, JwtHashAlgorithm.HS256));
        }
    }
}
