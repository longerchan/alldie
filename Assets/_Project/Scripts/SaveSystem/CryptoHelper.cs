using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FrostShelter.SaveSystem
{
    public static class CryptoHelper
    {
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("FrostShelterSalt!@#2024");
        private const int Iterations = 10000;
        private const int KeySize = 256;

        public static string Encrypt(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var key = DeriveKey(password);
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            // Write IV first
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText, string password)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            try
            {
                var cipherBytes = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var key = DeriveKey(password);
                aes.Key = key;

                // Read IV from first 16 bytes
                var iv = new byte[16];
                Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        public static string ComputeHash(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(bytes);
        }

        public static bool VerifyIntegrity(string data, string hash)
        {
            return ComputeHash(data) == hash;
        }

        private static byte[] DeriveKey(string password)
        {
            using var rfc2898 = new Rfc2898DeriveBytes(
                password,
                Salt,
                Iterations,
                HashAlgorithmName.SHA256);
            return rfc2898.GetBytes(KeySize / 8);
        }
    }
}
