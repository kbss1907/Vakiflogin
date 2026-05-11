using Microsoft.AspNetCore.Connections;
using Microsoft.Data.SqlClient;
using System.Data;
using VAKIFLOGIN.Models;
using VAKIFLOGIN.Services;
using VAKIFLOGIN.Controllers;
using VAKIFLOGIN.DTOs;
namespace VAKIFLOGIN.Data
{

    public class UserRepository : IUserRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public UserRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Güvenli kolon okuma: SP hangi kolonları dönerse dönsün, patlamaması için kontrol
        private static bool HasColumn(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static string SafeGetString(IDataReader reader, string columnName)
        {
            return HasColumn(reader, columnName) && reader[columnName] != DBNull.Value
                ? reader[columnName]?.ToString() ?? ""
                : "";
        }

        private static int SafeGetInt(IDataReader reader, string columnName, int defaultValue = 0)
        {
            return HasColumn(reader, columnName) && reader[columnName] != DBNull.Value
                ? Convert.ToInt32(reader[columnName])
                : defaultValue;
        }

        private static int? SafeGetNullableInt(IDataReader reader, string columnName)
        {
            return HasColumn(reader, columnName) && reader[columnName] != DBNull.Value
                ? Convert.ToInt32(reader[columnName])
                : null;
        }

        private static DateTime? SafeGetNullableDateTime(IDataReader reader, string columnName)
        {
            return HasColumn(reader, columnName) && reader[columnName] != DBNull.Value
                ? Convert.ToDateTime(reader[columnName])
                : null;
        }

        public void Register(User user)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_User_Create_HRMS";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = user.Email });
                command.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 255) { Value = user.FirstName });
                command.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 255) { Value = user.LastName });
                command.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 255) { Value = user.Phone });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.Int) { Value = user.DepartmentId });
                command.Parameters.Add(new SqlParameter("@PositionId", SqlDbType.Int) { Value = user.PositionId });
                command.Parameters.Add(new SqlParameter("@ManagerId", SqlDbType.Int) { Value = user.ManagerUserId == 0 ? (object)DBNull.Value : user.ManagerUserId });
                command.Parameters.Add(new SqlParameter("@CreatorAdminId", SqlDbType.Int) { Value = user.CreatorAdminId == 0 ? (object)DBNull.Value : user.CreatorAdminId });
                command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.VarBinary, -1) { Value = user.PasswordHash });
                command.Parameters.Add(new SqlParameter("@PasswordSalt", SqlDbType.VarBinary, -1) { Value = user.PasswordSalt });
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                command.ExecuteNonQuery();
            }


        }

        public User? Login(string email, string password)
        {

            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_GetUserByEmail"; // sp adı
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = email });
                

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (var reader = command.ExecuteReader())
                {

                    if (reader.Read())
                    {

                        var user = new User();

                        
                        user.Id = Convert.ToInt32(reader["Id"]);
                        user.Email = reader["Email"]?.ToString() ?? "";
                        user.PasswordHash = (byte[])reader["PasswordHash"];
                        user.PasswordSalt = (byte[])reader["PasswordSalt"];
                        user.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                        user.IsActive = Convert.ToBoolean(reader["IsActive"]);



                        return user;
                    }
                    else
                    {
                        
                        return null;
                    }
                }
            }


        }

        public void SaveRefreshToken(int userId, string token, DateTime expiresAt, string createdByIp)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_RefreshToken_Insert"; // Sizin SP adınız
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                command.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.VarBinary, -1) { Value = System.Text.Encoding.UTF8.GetBytes(token) });
                command.Parameters.Add(new SqlParameter("@ExpiresAt", SqlDbType.DateTime) { Value = expiresAt });
                command.Parameters.Add(new SqlParameter("@CreatedByIp", SqlDbType.NVarChar) { Value = createdByIp ?? (object)DBNull.Value });
                if (connection.State != ConnectionState.Open) connection.Open();
                command.ExecuteNonQuery();
            }
        }


        public User? GetUserByRefreshToken(int userId, string token)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_GetRefreshTokenGetValid"; 
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                command.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.VarBinary, -1) { Value = System.Text.Encoding.UTF8.GetBytes(token) });
                if (connection.State != ConnectionState.Open) connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var user = new User();
                        user.Id = Convert.ToInt32(reader["UserId"]);
                        return user;
                    }
                    return null;
                }
            }
        }



        public void RevokeRefreshToken(string token)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_RefreshToken_Revoke"; 
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@TokenHash", SqlDbType.VarBinary, -1) { Value = System.Text.Encoding.UTF8.GetBytes(token) });
                if (connection.State != ConnectionState.Open) connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<User> GetHRList()
        {
            var userList = new List<User>();
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_Users_GetHRList"; 
                command.CommandType = CommandType.StoredProcedure;
                if (connection.State != ConnectionState.Open) connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    
                    while (reader.Read())
                    {
                        var user = new User();
                        user.Id = Convert.ToInt32(reader["Id"]);
                        user.Email = reader["Email"]?.ToString() ?? "";
                        user.FirstName = reader["FirstName"]?.ToString() ?? "";
                        user.LastName = reader["LastName"]?.ToString() ?? "";
                        user.DepartmentId = Convert.ToInt32(reader["DepartmentId"]);
                        user.PositionId = Convert.ToInt32(reader["PositionId"]);
                        user.ManagerUserId = Convert.ToInt32(reader["ManagerUserId"]);
                        user.CreatorAdminId = Convert.ToInt32(reader["CreatorAdminId"]);
                        user.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                        user.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        user.UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]);
                        user.Status = reader["Status"]?.ToString() ?? "";
                        user.HireDate = Convert.ToDateTime(reader["HireDate"]);
                        userList.Add(user);
                    }
                    return userList;
                }
            }
        }
        public List <UserListDto> GetAllUserListDto()
        {
            var userListDto = new List<UserListDto>();
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_Users_GetAllWithDetails";
                command.CommandType = CommandType.StoredProcedure;
                if (connection.State != ConnectionState.Open) connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // 1- Nesneyi 'doğum anında' zorunlu alanlarıyla oluşturuyoruz:
                        var user = new UserListDto {
                            Email = reader["Email"]?.ToString() ?? "",
                            FirstName = reader["FirstName"]?.ToString() ?? "",
                            LastName = reader["LastName"]?.ToString() ?? "",
                            Phone = reader["Phone"]?.ToString() ?? "",
                            Status = reader["Status"]?.ToString() ?? "",
                            PositionName = reader["PositionName"]?.ToString() ?? "",
                            DepartmentName = reader["DepartmentName"]?.ToString() ?? ""
                        };
                        // 2- Zorunlu olmayan (int, DateTime vb.) alanlara aşağıda devam ediyoruz:
                        user.Id = Convert.ToInt32(reader["Id"]);
                        user.RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]);
                        user.RoleName = reader["RoleName"] == DBNull.Value ? "" : reader["RoleName"]?.ToString();
                        user.DepartmentId = reader["DepartmentId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DepartmentId"]);
                        user.PositionId = reader["PositionId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PositionId"]);
                        user.ManagerUserId = reader["ManagerUserId"] == DBNull.Value ? null : Convert.ToInt32(reader["ManagerUserId"]);
                        user.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                        user.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        user.HireDate = reader["HireDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["HireDate"]);
                        userListDto.Add(user);
                    }
                    return userListDto;
                }
            }
        }
        public UserListDto? GetUserById(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_Users_GetById";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = id });
                if (connection.State != ConnectionState.Open) connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var user = new UserListDto {
                            Email = reader["Email"]?.ToString() ?? "",
                            FirstName = reader["FirstName"]?.ToString() ?? "",
                            LastName = reader["LastName"]?.ToString() ?? "",
                            Phone = reader["Phone"]?.ToString() ?? "",
                            Status = reader["Status"]?.ToString() ?? "",
                            PositionName = reader["PositionName"]?.ToString() ?? "",
                            DepartmentName = reader["DepartmentName"]?.ToString() ?? ""
                        };
                        // 2- Zorunlu olmayan (int, DateTime vb.) alanlara aşağıda devam ediyoruz:
                        user.Id = Convert.ToInt32(reader["Id"]);
                        user.RoleId = reader["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RoleId"]);
                        user.RoleName = reader["RoleName"] == DBNull.Value ? "" : reader["RoleName"]?.ToString();
                        user.DepartmentId = reader["DepartmentId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DepartmentId"]);
                        user.PositionId = reader["PositionId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PositionId"]);
                        user.ManagerUserId = reader["ManagerUserId"] == DBNull.Value ? null : Convert.ToInt32(reader["ManagerUserId"]);
                        user.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                        user.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        user.HireDate = reader["HireDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["HireDate"]);
                        return user;
                    }
                    return null;
                }
            }
        }

        public void UpdateEmployee(int id, UpdateEmployeeDto dto)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_Users_Update";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = id });
                command.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 50) { Value = dto.FirstName ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 50) { Value = dto.LastName ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 20) { Value = dto.Phone });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.Int) { Value = dto.DepartmentId });
                command.Parameters.Add(new SqlParameter("@PositionId", SqlDbType.Int) { Value = dto.PositionId });
                command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = dto.Status ?? (object)DBNull.Value });
                if (connection.State != ConnectionState.Open) connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public List<UserListDto> SearchUser(string searchTerm, int? departmentId, string status){

            var result = new List<UserListDto>();
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_Users_Search";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 50) { Value = string.IsNullOrEmpty(searchTerm) ? (object)DBNull.Value : searchTerm });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.Int) { Value = departmentId ?? (object)DBNull.Value });
                command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = string.IsNullOrEmpty(status) ? (object)DBNull.Value : status });
                if (connection.State != ConnectionState.Open) connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new UserListDto {
                            Email = SafeGetString(reader, "Email"),
                            FirstName = SafeGetString(reader, "FirstName"),
                            LastName = SafeGetString(reader, "LastName"),
                            Phone = SafeGetString(reader, "Phone"),
                            Status = SafeGetString(reader, "Status"),
                            PositionName = SafeGetString(reader, "PositionName"),
                            DepartmentName = SafeGetString(reader, "DepartmentName")
                        };
                        user.Id = SafeGetInt(reader, "Id");
                        user.RoleId = SafeGetInt(reader, "RoleId");
                        user.RoleName = SafeGetString(reader, "RoleName");
                        user.DepartmentId = SafeGetInt(reader, "DepartmentId");
                        user.PositionId = SafeGetInt(reader, "PositionId");
                        user.ManagerUserId = SafeGetNullableInt(reader, "ManagerUserId");
                        user.CreatedAt = SafeGetNullableDateTime(reader, "CreatedAt") ?? DateTime.MinValue;
                        user.IsActive = HasColumn(reader, "IsActive") && reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]);
                        user.HireDate = SafeGetNullableDateTime(reader, "HireDate");
                        result.Add(user);
                    }
                    return result;
                }
            }
        }

        
        public void AssignRole(int userId, int roleId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_UserRoles_Assign";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                command.Parameters.Add(new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId });
                if (connection.State != ConnectionState.Open) connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void SavePasswordResetToken(string email, string token , DateTime  expiresAt){
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_Users_SavePasswordResetToken";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 50) { Value = email });
                command.Parameters.Add(new SqlParameter("@TokenText", SqlDbType.NVarChar, 255) { Value = token });
                command.Parameters.Add(new SqlParameter("@ExpiresAt", SqlDbType.DateTime) { Value = expiresAt });
                if (connection.State != ConnectionState.Open) connection.Open();
                var result = command.ExecuteScalar();
                if (result == null || Convert.ToInt32(result) == 0)
                {
                    throw new Exception("Bu email adresine ait aktif bir kullanıcı bulunamadı.");
                }
            }
        }
        public string? GetEmailByPasswordResetToken( string token){
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_PasswordResetToken_GetByToken";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@TokenText", SqlDbType.NVarChar, 255) { Value = token });
                if (connection.State != ConnectionState.Open) connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["Email"]?.ToString() ?? "";
                    }
                    return null;
                }
            }
        }
        public void UpdateUserPassword( string email, byte[] passwordHash, byte[] passwordSalt){
            using (var connection = _connectionFactory.CreateConnection())
            using (var command = ((SqlConnection)connection).CreateCommand())
            {
                command.CommandText = "dbo.sp_Users_UpdatePassword";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 50) { Value = email });
                command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.VarBinary, -1) { Value = passwordHash });
                command.Parameters.Add(new SqlParameter("@PasswordSalt", SqlDbType.VarBinary, -1) { Value = passwordSalt });
                if (connection.State != ConnectionState.Open) connection.Open();
                var result = command.ExecuteScalar();
                if (result == null || Convert.ToInt32(result) == 0)
                {
                    throw new Exception("Şifre güncellenecek kullanıcı bulunamadı.");
                }
            }
        }


    



    }
}
