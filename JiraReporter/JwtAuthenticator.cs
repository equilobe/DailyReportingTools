using JWT;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class JwtAuthenticator : IAuthenticator
    {
        private readonly string jwtToken;

        public JwtAuthenticator(RestRequest request, string sharedSecret)
        {
            var path = request.Resource.Split('?')[0];
            if (!path.StartsWith("/"))
                path = "/" + path;

            var unhashedQshClaim = string.Format("{0}&{1}&", request.Method.ToString().ToUpper(), path);

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
            tokenBuilder.Claims.Add("iss", "com.equilobe.drt");

            jwtToken = tokenBuilder.Encode().JwtTokenString;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddParameter("jwt", jwtToken, ParameterType.QueryString);
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
