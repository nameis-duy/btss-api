using BCrypt.Net;
using Infrastructure.Constants;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Utilities
{
    public static class StringUtil
    {
        public static string Hash(this string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, HashType.SHA512);
        }
        public static string HashWithNoSalt(this string origin)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(origin));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
        public static bool VerifyHashString(this string hash, string password, HashType type = HashType.SHA512)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hash, type);
        }
        public static string RemoveDiacritics(this string input)
        {
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);
            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().ToLower();
        }
        public static string CreateThumbnailLink(this string imagePath)
        {
            return $"{GlobalConstants.IMAGE_SOURCE}{GlobalConstants.IMAGE_THUMB_SIZE}{imagePath}";
        }

    }
}
