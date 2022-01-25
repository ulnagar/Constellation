using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Constellation.Application.Extensions
{
    public static class PasswordExtensions
    {
        public static string DecodePassword(this string code, string salt)
        {
            var passArray = Convert.FromBase64String(code);

            var hashmd5 = new MD5CryptoServiceProvider();
            var saltArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(salt));
            hashmd5.Clear();

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = saltArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = tdes.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(passArray, 0, passArray.Length);
            tdes.Clear();

            return Encoding.UTF8.GetString(resultArray);
        }

        public static string EncodePassword(this string pass, string salt)
        {
            var passArray = Encoding.UTF8.GetBytes(pass);

            var hashmd5 = new MD5CryptoServiceProvider();
            var saltArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(salt));
            hashmd5.Clear();

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = saltArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = tdes.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(passArray, 0, passArray.Length);
            tdes.Clear();

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string GenerateSalt(this string dummy, int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GeneratePassword(this string dummy, int length, int numberOfNonAlphaCharacters)
        {
            if (length < 1 || length > 128)
                throw new ArgumentException(nameof(length));

            if (numberOfNonAlphaCharacters > length || numberOfNonAlphaCharacters < 0)
                throw new ArgumentException(nameof(numberOfNonAlphaCharacters));

            char[] punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

            using (var rng = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[length];

                rng.GetBytes(byteBuffer);
                var count = 0;
                var characterBuffer = new char[length];

                for (var iter = 0; iter < length; iter++)
                {
                    var i = byteBuffer[iter] % 87;

                    if (i < 10)
                        characterBuffer[iter] = (char)('0' + i);
                    else if (i < 36)
                        characterBuffer[iter] = (char)('A' + i - 10);
                    else if (i < 62)
                        characterBuffer[iter] = (char)('a' + i - 36);
                    else
                    {
                        characterBuffer[iter] = punctuations[i - 62];
                        count++;
                    }
                }

                if (count >= numberOfNonAlphaCharacters)
                    return new string(characterBuffer);

                int j;
                var rand = new Random();

                for (j = 0; j < numberOfNonAlphaCharacters - count; j++)
                {
                    int k;
                    do
                    {
                        k = rand.Next(0, length);
                    }
                    while (!char.IsLetterOrDigit(characterBuffer[k]));

                    characterBuffer[k] = punctuations[rand.Next(0, punctuations.Length)];
                }

                return new string(characterBuffer);
            }
        }
    }
}
