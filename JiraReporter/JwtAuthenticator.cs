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

namespace JiraReporter
{
    public class JwtAuthenticator : IAuthenticator
    {
        private readonly string _jwtToken;

        public JwtAuthenticator(string jwtToken)
        {
            _jwtToken = jwtToken;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddParameter("jwt", _jwtToken, ParameterType.UrlSegment);
        }

        public static string CreateJwt(string addonKey, string sharedSecret, string query, string method)
        {
            var path = query.Split('?')[0];
            if (!path.StartsWith("/"))
                path = "/" + path;

            var queryString = HttpUtility.ParseQueryString(query);
            var sortedQueryStringKeys = queryString
                .Cast<string>()
                .OrderBy(x => x)
                .ToList();

            var queryStringsForClaim = sortedQueryStringKeys
                .Where(x => x != "jwt")
                .Select(x => string.Format("{0}={1}", x, HttpUtility.UrlEncode(queryString[x])));

            var queryStringClaim = String.Join("&", queryStringsForClaim);
            queryStringClaim = new Regex(@"%[a-f0-9]{2}").Replace(queryStringClaim, m => m.Value.ToUpperInvariant());

            var unhashedQshClaim = string.Format("{0}&{1}&{2}", method, path, queryStringClaim);

            string hashedQshClaim;
            using (var sha = SHA256.Create())
            {
                var computedHash = sha.ComputeHash(Encoding.UTF8.GetBytes(unhashedQshClaim));
                hashedQshClaim = BitConverter.ToString(computedHash).Replace("-", "").ToLower();
            }

            var tokenBuilder = new DecodedJwtToken(sharedSecret);
            tokenBuilder.Claims.Add("qsh", hashedQshClaim);
            tokenBuilder.Claims.Add("iat", DateTime.UtcNow.AsUnixTimestampSeconds());
            tokenBuilder.Claims.Add("exp", DateTime.UtcNow.AddMinutes(5).AsUnixTimestampSeconds());
            tokenBuilder.Claims.Add("iss", addonKey);

            return tokenBuilder.Encode().JwtTokenString;
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
