using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FNRC_DigitalHub.Helper
{
    public class SSOTokenService
    {
        private readonly string _secret = "7fYg0N/pFZ6JfG/ThTQgPrKAxYU=|1|qxbqYtpfukpOWYVenU2s4Q==";
        private readonly string _issuer = "Digital Hub";
        private readonly string _audience = "FNRC-Apps";

        public string GenerateToken(string employeeId, string username, string displayName, int timeInMin = 1)
        {
            var claims = new[]
            {
                new Claim("EmployeeId", employeeId),
                new Claim(ClaimTypes.Name, username),
                new Claim("DisplayName", displayName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(timeInMin),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}