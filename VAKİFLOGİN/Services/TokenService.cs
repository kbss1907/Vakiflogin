using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VAKIFLOGIN.Models;
using System.Security.Cryptography;
namespace VAKIFLOGIN.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Settings: Key is missing from configuration!");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));


            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(20), // Kartın geçerlilik süresi 20 dakika
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Settings: Issuer is missing from configuration!"),
                Audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Settings: Audience is missing from configuration!")
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

    }
}
