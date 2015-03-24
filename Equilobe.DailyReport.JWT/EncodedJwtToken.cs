using JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.JWT
{
    public class EncodedJwtToken
    {
        public EncodedJwtToken(string sharedSecret, string jwtToken)
        {
            SharedSecret = sharedSecret;
            JwtTokenString = jwtToken;
        }

        public string SharedSecret { get; set; }
        public string JwtTokenString { get; set; }

        public DecodedJwtToken Decode()
        {
            return new DecodedJwtToken(SharedSecret)
            {
                Claims = JsonWebToken.DecodeToObject(JwtTokenString, SharedSecret) as IDictionary<string, object>
            };
        }
    }
}
