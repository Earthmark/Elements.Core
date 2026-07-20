using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using OtpNet;

namespace Elements.Core
{
    public static class CryptoHelper
    {
        public readonly static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public static byte[] GenerateCryptoBlob(int length)
        {
            byte[] data = new byte[length];
            rng.GetBytes(data);
            return data;
        }

        public static int CryptoTokenStringLength(int length) => ((4 * length / 3) + 3) & ~3;

        public static string GenerateReadableCryptoToken(int length = 12)
        {
            var cryptoBlob = GenerateCryptoBlob(length);
            return Base32Encoding.ToString(cryptoBlob).Replace("=", "");
        }

        public static string GenerateCryptoToken(int length = 128)
        {
            var cryptoBlob = GenerateCryptoBlob(length);
            var cryptoToken = Convert.ToBase64String(cryptoBlob, Base64FormattingOptions.None);

            cryptoToken = cryptoToken.Replace('/', '_');

            return cryptoToken;
        }

        public static string GenerateSalt() => Convert.ToBase64String(GenerateCryptoBlob(128));
        public static string GenerateClientSecret() => Convert.ToBase64String(GenerateCryptoBlob(128));

        public static byte[] HashID(string id)
        {
            var bytes = Encoding.UTF8.GetBytes(id);
            return HashData(bytes);
        }

        public static byte[] HashData(byte[] data)
        {
            using(var sha256 = new SHA256Managed())
                return sha256.ComputeHash(data);
        }

        public static string HashIDToToken(string id, string salt = null)
        {
            var hash = HashID(id + salt);

            return BitConverter.ToString(hash).Replace("-", "");
        }

        public static string HashPassword(string password, string salt)
        {
            var passBytes = Encoding.UTF8.GetBytes(password);
            var saltBytes = Convert.FromBase64String(salt);

            var code = new byte[passBytes.Length + saltBytes.Length];

            for (int i = 0; i < passBytes.Length; i++)
                code[i] = passBytes[i];

            for (int i = 0; i < saltBytes.Length; i++)
                code[i + passBytes.Length] = saltBytes[i];

            using(var sha256 = new SHA256Managed())
            { 
                var hash = sha256.ComputeHash(code);

                return Convert.ToBase64String(hash);
            }
        }

        public static string PasswordRuleDescription => @"Minimum 8 characters, 1 capital letter and 1 digit.";

        public static bool IsValidPassword(string password)
        {
            if (password == null)
                return false;

            if (password.Length < 8)
                return false;

            // count numbers
            if (password.Count(c => char.IsDigit(c)) == 0 ||
               password.Count(c => char.IsLetter(c)) == 0 ||
               password.Count(c => char.IsLower(c)) == 0 ||
               password.Count(c => char.IsUpper(c)) == 0)
                return false;

            return true;
        }

        public static string PasswordRequirements => "Must have at least 8 symbols, 1 digit and 1 uppercase letter";
    }
}
