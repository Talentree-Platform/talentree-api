using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using Talentree.Service.Contracts;

namespace Talentree.Service.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(IConfiguration configuration)
        {
            var secret = configuration["Encryption:Key"]
                ?? throw new InvalidOperationException("Encryption:Key is missing in appsettings");

            // Derive key and IV from the secret
            using var sha256 = SHA256.Create();
            _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(secret));
            _iv = _key[..16]; // use first 16 bytes as IV
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            var bytes = Encoding.UTF8.GetBytes(plainText);
            cs.Write(bytes, 0, bytes.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var bytes = Convert.FromBase64String(cipherText);
            using var ms = new MemoryStream(bytes);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);

            return reader.ReadToEnd();
        }

        public string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
                return "****";

            var last4 = accountNumber[^4..];
            return $"****{last4}";
        }
    }
}