using System.Security.Cryptography;
using System.Text;
namespace VAKIFLOGIN.Services
{
    public static class EncryptionHelper
    {
        public static string Decrypt(string encryptedBase64, string privateKeyBase64)
        {
            if (string.IsNullOrEmpty(encryptedBase64)) return encryptedBase64;
            try
            {
                using (var rsa = RSA.Create())
                {
                    // Private key'i yüklüyoruz
                    rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKeyBase64), out _);
                    
                    byte[] encryptedData = Convert.FromBase64String(encryptedBase64);
                    
                    // Web Crypto API (Frontend) varsayılan olarak OAEP SHA-256 kullanır
                    byte[] decryptedData = rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
                    
                    return Encoding.UTF8.GetString(decryptedData);
                }
            }
            catch (Exception)
            {
                // Çözme başarısız olursa (örneğin veri şifreli değilse) hatayı logla 
                return encryptedBase64; 
            }
        }
    }
}