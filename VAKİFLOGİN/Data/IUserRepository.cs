using VAKIFLOGIN.Models;
using VAKIFLOGIN.DTOs;
namespace VAKIFLOGIN.Data
{
    public interface IUserRepository
    {
        void Register(User user);
        void SaveRefreshToken(int userId, string token, DateTime expiresAt, string createdByIp);
        User? GetUserByRefreshToken(int userId, string token);
        void RevokeRefreshToken(string token);
        User? Login(string email, string password);
        List<User> GetHRList();//personel getirme listesi imzası 
        List<UserListDto> GetAllUserListDto();
        UserListDto? GetUserById(int id);
        void UpdateEmployee(int id, UpdateEmployeeDto dto);
        List<UserListDto> SearchUser(string searchTerm, int? departmentId, string status);
        void AssignRole(int userId, int roleId);
        void SavePasswordResetToken(string email, string token , DateTime  expiresAt);
        string? GetEmailByPasswordResetToken( string token);
        void UpdateUserPassword( string email, byte[] passwordHash, byte[] passwordSalt);
    }



}