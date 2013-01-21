using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using CoreSystem.RefTypeExtension;
using CoreSystem.ValueTypeExtension;

namespace CoreSystem.Crypto
{
    /// <summary>
    /// Class is used for encrypting and descryption
    /// </summary>
    public class Cipher
    {
        private static byte[] StaticKey = new byte[] { 0x7A, 0x91, 0xDD, 0x5E, 0x2B, 0xBF, 0x60, 0x9E, 0x76, 0xE0, 0x8D, 0x92, 0x016, 0x7E, 0xA5, 0x55 };

        TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();

        /// <summary>
        /// Default constructor that initializes object with static key
        /// </summary>
        /// <remarks>Default constructor only initializes with same key, 
        /// so it is not good for production</remarks>
        public Cipher()
            : this(StaticKey)
        { }


        /// <summary>
        /// Constructor that initializes object with encryption key
        /// </summary>
        /// <param name="key">Encyrption key</param>
        public Cipher(byte[] key)
        {
            tripleDES.Key = key;
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
        }

        /// <summary>
        /// Encrypt clear string and returns hex string representation
        /// </summary>
        /// <param name="clearString">Text to encrypt</param>
        /// <param name="useSalt">Use random initialization vector for encryption</param>
        /// <returns>Hex representation of encrypted value</returns>
        public string Encrypt(string clearString, bool useSalt = false)
        {
            byte[] clearBytes = UTF8Encoding.Unicode.GetBytes(clearString);

            if (useSalt)
            {
                var salt = PasswordHelper.GenerateSalt();
                using (ICryptoTransform cTransform = tripleDES.CreateEncryptor(tripleDES.Key, salt.HexToBytes()))
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(clearBytes, 0, clearBytes.Length);
                    return salt + "$" + resultArray.ToHexString();
                }
            }

            using (ICryptoTransform cTransform = tripleDES.CreateEncryptor())
            {
                byte[] resultArray = cTransform.TransformFinalBlock(clearBytes, 0, clearBytes.Length);
                return resultArray.ToHexString();
            }
        }

        /// <summary>
        /// Decrypt encrypted string
        /// </summary>
        /// <param name="encString">Hex representation of encrypted value</param>
        /// <returns>Clear/descrypted string</returns>
        public string Decrypt(string encString)
        {
            byte[] encBytes;

            if (encString.Contains('$'))
            {
                var parts = encString.Split('$');
                byte[] salt = parts[0].HexToBytes();
                encBytes = parts[1].HexToBytes();

                using (ICryptoTransform cTransform = tripleDES.CreateDecryptor(tripleDES.Key, salt))
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(encBytes, 0, encBytes.Length);
                    return UTF8Encoding.Unicode.GetString(resultArray);
                }
            }

            encBytes = encString.HexToBytes();

            using (ICryptoTransform cTransform = tripleDES.CreateDecryptor())
            {
                byte[] resultArray = cTransform.TransformFinalBlock(encBytes, 0, encBytes.Length);
                return UTF8Encoding.Unicode.GetString(resultArray);
            }
        }
    }
}
