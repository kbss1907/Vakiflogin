using System.Security.Cryptography;
using System.Text;

namespace VAKIFLOGIN.Services
{
    public static class PasswordHelper
    {
        //"out" anahtar kelimesi, bir metottan birden fazla değer döndürmemizi sağlar.
        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

        }
        public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                if (storedHash.Length != computedHash.Length) return false;
                // Ürettiğimiz yeni hash ile veritabanından gelen hash'i byte byte karşılaştırıyoruz.
                for (int i = 0; i < computedHash.Length; i++)
                {
                    
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            
            return true;
        }
    }
}
