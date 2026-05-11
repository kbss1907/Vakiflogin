using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace VAKIFLOGIN.Data
{
    public class DbConnectionFactory
    {
        private readonly IConfiguration _configuration; 

        public DbConnectionFactory(IConfiguration configuration)// burada bir constructor kullanarak app settinge ulaşma şekli
        {
            _configuration = configuration;
                
        }

        public IDbConnection CreateConnection()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' bulunamadı. appsettings.json veya ortam değişkenlerini kontrol edin.");
            }

            return new SqlConnection(connectionString);
        }
    }
}



