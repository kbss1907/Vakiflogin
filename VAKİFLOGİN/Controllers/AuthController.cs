using Microsoft.AspNetCore.Mvc;
using VAKIFLOGIN.Data;   
using VAKIFLOGIN.Models;
using VAKIFLOGIN.Services;
using VAKIFLOGIN.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace VAKIFLOGIN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _env;

        public AuthController(IUserRepository userRepository, ITokenService tokenService, IConfiguration configuration, IMemoryCache cache, IWebHostEnvironment env)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _cache = cache;
            _env = env;
        }

        [HttpPost("register")]
        [EnableRateLimiting("loginPolicy")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            var privateKey = _configuration["RsaSettings:PrivateKey"] ?? throw new InvalidOperationException("RSA Settings: PrivateKey is missing from configuration!");
            dto.Password = EncryptionHelper.Decrypt(dto.Password, privateKey);
            PasswordHelper.CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);
            
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                ManagerUserId = dto.ManagerUserId,
                CreatorAdminId = dto.CreatorAdminId,
                CreatedAt = dto.CreatedAt,
                IsActive = dto.IsActive,
                HireDate = dto.HireDate
            };
            
            _userRepository.Register(user);
            return Ok(new { message = "Kayıt başarıyla oluşturuldu." });
        }

        [HttpPost("login")]
        [EnableRateLimiting("loginPolicy")]
        
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var privateKey = _configuration["RsaSettings:PrivateKey"] ?? throw new InvalidOperationException("RSA Settings: PrivateKey is missing from configuration!");
            loginDto.Password = EncryptionHelper.Decrypt(loginDto.Password, privateKey);
            
            try
            {
                var user = _userRepository.Login(loginDto.Email, loginDto.Password);
                if (user == null || !PasswordHelper.VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return Unauthorized(new { message = "Hatalı email veya şifre!" });
                }
                  
                var accessToken = _tokenService.CreateToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();
                var userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                // STATEFUL BFF: JWT Sunucuda Kalır
                var sessionId = Guid.NewGuid().ToString();
                _cache.Set(sessionId, new { AccessToken = accessToken, IP = userIp, UserId = user.Id, Email = user.Email }, TimeSpan.FromHours(1));
                
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var refreshTokenExpiry = loginDto.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(1);
                _userRepository.SaveRefreshToken(user.Id, refreshToken, refreshTokenExpiry, ipAddress);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_env.IsDevelopment(), 
                    SameSite = SameSiteMode.Lax,
                    Expires = refreshTokenExpiry 
                }; 

                Response.Cookies.Append("VakifSessionId", sessionId, cookieOptions);
                Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
                
                return Ok(new
                {
                    message = "Giriş Başarılı!",
                    userInfo = new { id = user.Id, email = user.Email, fullName = $"{user.FirstName} {user.LastName}" }
                });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Giriş işlemi sırasında sistemsel bir hata oluştu." });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            var user = _userRepository.GetUserById(int.Parse(userIdClaim.Value));
            if (user == null) return NotFound();
            
            return Ok(new { id = user.Id, email = user.Email, firstName = user.FirstName, lastName = user.LastName });
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var refreshTokenCookie = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshTokenCookie)) return Unauthorized();

            // Session cache'den userId ve email bilgisini al
            var sessionCookie = Request.Cookies["VakifSessionId"];
            if (string.IsNullOrEmpty(sessionCookie)) return Unauthorized();
            
            dynamic? sessionData = _cache.Get(sessionCookie);
            if (sessionData == null) return Unauthorized();
            
            int userId = (int)sessionData.UserId;
            string userEmail = (string)sessionData.Email;

            var user = _userRepository.GetUserByRefreshToken(userId, refreshTokenCookie);
            if (user == null) return Unauthorized();

            // SP Email dönmüyor, cache'den aldığımız email'i set et
            user.Email = userEmail;

            _userRepository.RevokeRefreshToken(refreshTokenCookie);
            var newAccessToken = _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var sessionId = Guid.NewGuid().ToString();
            _cache.Set(sessionId, new { AccessToken = newAccessToken, IP = userIp, UserId = user.Id, Email = user.Email }, TimeSpan.FromHours(1));
            
            _userRepository.SaveRefreshToken(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(7), "127.0.0.1");

            var cookieOptions = new CookieOptions { HttpOnly = true, Secure = !_env.IsDevelopment(), SameSite = SameSiteMode.Lax, Expires = DateTime.UtcNow.AddDays(7) };
            Response.Cookies.Append("VakifSessionId", sessionId, cookieOptions);
            Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

            return Ok(new { message = "Oturum yenilendi" });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email))
                return BadRequest(new { message = "Email adresi zorunludur." });
                
            string resetToken = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(); 
            DateTime expiresAt = DateTime.UtcNow.AddHours(1);
            
            try
            {
                _userRepository.SavePasswordResetToken(dto.Email, resetToken, expiresAt);
                // TODO: Gerçek ortamda bu kodu email/SMS ile gönderin
                // Geliştirme ortamında kodu sadece Development modunda loglayın
                if (_env.IsDevelopment())
                {
                    Console.WriteLine($"[DEV ONLY] Şifre sıfırlama kodu: {resetToken}");
                }
                return Ok(new { message = "Şifre sıfırlama kodu email adresinize gönderildi." });
            }
            catch (Exception) 
            {
                return BadRequest(new { message = "Sistemsel bir hata oluştu." });
            }
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var privateKey = _configuration["RsaSettings:PrivateKey"] ?? throw new InvalidOperationException("RSA Settings: PrivateKey is missing from configuration!");
            dto.NewPassword = EncryptionHelper.Decrypt(dto.NewPassword, privateKey);
            dto.ConfirmPassword = EncryptionHelper.Decrypt(dto.ConfirmPassword, privateKey);
            
            try
            {
                if (dto.NewPassword != dto.ConfirmPassword)
                    return BadRequest(new { message = "Şifreler birbiriyle eşleşmiyor." });
                    
                string? email = _userRepository.GetEmailByPasswordResetToken(dto.Token);
                if (string.IsNullOrEmpty(email))
                    return BadRequest(new { message = "Kod geçersiz veya süresi dolmuş." });
                    
                PasswordHelper.CreatePasswordHash(dto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                _userRepository.UpdateUserPassword(email, passwordHash, passwordSalt);
                
                return Ok(new { message = "Şifreniz başarıyla güncellendi." });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "İşlem başarısız." });
            }
        }

        
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var sessionId = Request.Cookies["VakifSessionId"];
            if (!string.IsNullOrEmpty(sessionId)) _cache.Remove(sessionId);

            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken)) _userRepository.RevokeRefreshToken(refreshToken);
            var cookieOptions = new CookieOptions 
            { 
                Path = "/", // Çerezin nerede oluşturulduğunu hatırlatıyoruz
                HttpOnly = true, 
                Secure = !_env.IsDevelopment(), 
                SameSite = SameSiteMode.Lax 
            };
            Response.Cookies.Delete("VakifSessionId",cookieOptions);
            Response.Cookies.Delete("refreshToken",cookieOptions);
            return Ok(new { message = "Çıkış yapıldı" });
        }
    }
}