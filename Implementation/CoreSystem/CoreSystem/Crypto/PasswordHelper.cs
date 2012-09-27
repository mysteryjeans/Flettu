using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using CoreSystem.RefTypeExtension;
using CoreSystem.Util;

namespace CoreSystem.Crypto
{
    /// <summary>
    /// Provide hash values to store in password, ensuring medium level security for sensitive information
    /// </summary>
    public static class PasswordHelper
    {
        private static readonly Random random = new Random((int)DateTime.Now.Ticks);
        private static readonly MD5 hashProvider = MD5.Create();

        /// <summary>
        /// Generate random hash value to store against password
        /// </summary>
        /// <param name="password">String to encrypt</param>
        /// <param name="salt">Random string to salt computed hash</param>
        /// <returns>Hash value for the password with the addition of salt</returns>
        public static string GenerateHash(string password, string salt = null)
        {
            Guard.CheckNullOrTrimEmpty(password, "Password cannot be empty");

            salt = salt ?? GenerateSalt();

            var bytes = Encoding.Unicode.GetBytes(salt + password);
            var hash = hashProvider.ComputeHash(bytes);

            return salt + "$" + hash.ToHexString();
        }

        /// <summary>
        /// Validate password is equal to hashValue(Generated from Compute hash)
        /// </summary>
        /// <param name="hashValue">Computed hash value of actual password</param>
        /// <param name="password">Password to validate against hash value</param>
        /// <returns>True if password is equal to the hash value</returns>
        public static bool Validate(string hashValue, string password)
        {
            Guard.CheckNullOrTrimEmpty(hashValue, "HashValue cannot be empty");
            
            if (!hashValue.Contains('$'))
                throw new ArgumentException("hashValue should contain salt seperated by '$'");

            var salt = hashValue.Split('$')[0];
            return hashValue == GenerateHash(password, salt);
        }

        /// <summary>
        /// Random salt to comsume in hash generation
        /// </summary>
        /// <param name="length">Length of salt value should be even</param>
        /// <returns>Salt value</returns>
        public static string GenerateSalt(int length=8)
        {
            var salt = new byte[length/2];
            random.NextBytes(salt);

            return salt.ToHexString();
        }

        /// <summary>
        /// Generate random string to be used as passwords and salts
        /// </summary>
        /// <returns>Base 64 random string</returns>
        public static string GeneratePassword()
        {
            var randomNumber = (random.Next(5000, int.MaxValue));
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(randomNumber.ToString()));
        }
    }
}
