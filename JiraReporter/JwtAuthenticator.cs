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
			var result = new StringBuilder();

			result.Append(method.ToUpperInvariant());

			result.Append("&");

			result.Append(GetPath(relativeUrl));

			result.Append("&");

			var canonicalQueryString = GetSortedQueryString(relativeUrl);
			if (!String.IsNullOrEmpty(canonicalQueryString))
			{
				result.Append(canonicalQueryString);
			}

			return result.ToString();
		}

		public static string CalculateHash(string canonicalRequest)
		{
			using (var sha = SHA256.Create())
			{
				var computedHash = sha.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest));
				return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
			}
		}

        private static string GetSortedQueryString(string relativeUrl)
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

		private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };
		internal static string EscapeUriDataStringRfc3986(string value)
		{
			StringBuilder escaped = new StringBuilder(Uri.EscapeDataString(value));
			for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
			{
				escaped.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
			}

			return escaped.ToString();
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
