using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Utilities
{
    public static class JwtUtil
    {
        public static string GenerateJWT(IEnumerable<Claim> claims,
                                         DateTime now,
                                         int minuteValid,
                                         IConfiguration config,
                                         bool useHmacSha512 = false,
                                         string? secretKey = null)
        {
            var encodedKey = Encoding.UTF8.GetBytes(secretKey ?? config["Jwt:Key"]!);
            var securityKey = new SymmetricSecurityKey(encodedKey);
            var credentials = new SigningCredentials(securityKey,
                                                     useHmacSha512 ? SecurityAlgorithms.HmacSha512 : SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(config["Jwt:Issuer"],
                                             config["Jwt:Audience"],
                                             claims,
                                             expires: now.AddMinutes(minuteValid),
                                             signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static bool VerifyJWT(string token,
                                     IConfiguration config,
                                     out IEnumerable<Claim> claims,
                                     bool useHmacSha512 = false,
                                     string? secretKey = null)
        {
            var encodedKey = Encoding.UTF8.GetBytes(secretKey ?? config["Jwt:Key"]!);
            var securityKey = new SymmetricSecurityKey(encodedKey);
            var validateParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = securityKey
            };
            var handler = new JwtSecurityTokenHandler();
            try
            {
                claims = handler.ValidateToken(token, validateParams, out SecurityToken securityToken).Claims
                                .Where(c => c.Type != "aud");
                var jwt = (JwtSecurityToken)securityToken;
                if (jwt == null ||
                    !jwt.Header.Alg.Equals(useHmacSha512 ? SecurityAlgorithms.HmacSha512 : SecurityAlgorithms.HmacSha256,
                                           StringComparison.InvariantCultureIgnoreCase))
                    return false;
                return true;
            }
            catch
            {
                claims = [];
                return false;
            }
        }
    }
}
