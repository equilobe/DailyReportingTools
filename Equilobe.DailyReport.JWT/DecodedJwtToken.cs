using JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Equilobe.DailyReport.JWT
{
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

        public void ValidateToken(HttpRequestBase request)
        {
            ValidateTokenHasNotExpired();
            ValidateQueryStringHash(request);
        }

        private void ValidateTokenHasNotExpired()
        {
            var expiresAt = Convert.ToInt64(Claims["exp"]);

            var now = DateTime.UtcNow.Millisecond / 1000L;

            if (expiresAt < now)
                throw new Exception("JWT Token Expired");
        }

        private void ValidateQueryStringHash(HttpRequestBase request)
        {
            var qsh = JwtAuthenticator.CalculateHash(request.Url.PathAndQuery, request.HttpMethod);
            var requestQsh = Claims["qsh"] as string;

            if (qsh != requestQsh)
                throw new Exception("QSH Does not match.");
        }
    }
}
