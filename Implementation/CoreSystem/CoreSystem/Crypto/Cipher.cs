using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace CoreSystem.Crypto
{
    /// <summary>
    /// Class is used for encrypting and descryption
    /// </summary>
    public class Cipher
    {
        private static byte[] StaticKey = new byte[] { 0x66, 0x91, 0xE0, 0x8D, 0x92, 0x088, 0x7E, 0xBB, 0x5E, 0x2B, 0xBF, 0x60, 0x9E, 0x76, 0xA5, 0x55 };

        TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();

        public Cipher()
            : this(StaticKey)
        { }

        public Cipher(byte[] key)
        {
            tripleDES.Key = key;
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
        }

        public string Encrypt(string clearString)
        {
            byte[] inputArray = UTF8Encoding.UTF8.GetBytes(clearString);
            
            using (ICryptoTransform cTransform = tripleDES.CreateEncryptor())
            {
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
        }

        public string Decrypt(string encString)
        {
            byte[] inputArray = Convert.FromBase64String(encString);
            
            using (ICryptoTransform cTransform = tripleDES.CreateDecryptor())
            {
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleDES.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
        }
    }
}
