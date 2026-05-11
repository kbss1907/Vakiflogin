using VAKIFLOGIN.Models;

namespace VAKIFLOGIN.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
        string GenerateRefreshToken();
    }
}
