using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using CoreSystem.RefTypeExtension;
using CoreSystem.ValueTypeExtension;
using CoreSystem.Util;

namespace CoreSystem.Crypto
{
    /// <summary>
    /// Cipher algorithm
    /// </summary>
    public enum CipherAlgo
    {
        TripleDES = 0,
        AES
    }

    /// <summary>
    /// Class is used for encrypting and descryption
    /// </summary>
    public class Cipher : IDisposable
    {
        private static byte[] StaticKey = new byte[] { 0x7A, 0x91, 0xDD, 0x5E, 0x2B, 0xBF, 0x60, 0x9E, 0x76, 0xE0, 0x8D, 0x92, 0x016, 0x7E, 0xA5, 0x55 };

        private SymmetricAlgorithm encrypto;

        /// <summary>
        /// Cipher encryption algorithm
        /// </summary>
        public CipherAlgo CipherAlgo { get; private set; }

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
        /// <param name="key">Encyrption key for algorithm must be in valid size for cipher algorigthm</param>
        /// <param name="useSalt">Use random initialization vector for encryption</param>
        /// <param name="cipherAlgo">Algorithm used for encrytion</param>
        public Cipher(byte[] key, bool useSalt=false, CipherAlgo cipherAlgo = CipherAlgo.TripleDES)
        {
            Guard.CheckNull(key, "Cipher(key)");

            switch (cipherAlgo)
            {
                case CipherAlgo.TripleDES:
                    this.encrypto = new TripleDESCryptoServiceProvider();
                    break;
                case CipherAlgo.AES:
                    this.encrypto = new AesManaged();
                    break;
                default:
                    throw new NotSupportedException(string.Format("Cipher algorithm not supported: {0}", cipherAlgo));
            }

            this.CipherAlgo = cipherAlgo;
            this.encrypto.Key = key;
            this.encrypto.Padding = PaddingMode.PKCS7;
            this.encrypto.Mode = useSalt ? CipherMode.CBC : CipherMode.ECB;
        }

        /// <summary>
        /// Encrypt clear string and returns hex string representation
        /// </summary>
        /// <param name="clearString">Text to encrypt</param>
        /// <returns>Hex representation of encrypted value</returns>
        public string Encrypt(string clearString)
        {
            byte[] clearBytes = UTF8Encoding.Unicode.GetBytes(clearString);

            if (this.encrypto.Mode == CipherMode.CBC)
            {
                var salt = PasswordHelper.GenerateSaltBytes(16);
                using (ICryptoTransform cTransform = encrypto.CreateEncryptor(encrypto.Key, salt))
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(clearBytes, 0, clearBytes.Length);
                    return this.CipherAlgo + "$" + salt.ToHexString() + "$" + resultArray.ToHexString();
                }
            }

            using (ICryptoTransform cTransform = encrypto.CreateEncryptor())
            {
                byte[] resultArray = cTransform.TransformFinalBlock(clearBytes, 0, clearBytes.Length);
                return this.CipherAlgo + "$$" + resultArray.ToHexString();
            }
        }

        /// <summary>
        /// Decrypt encrypted string
        /// </summary>
        /// <param name="encValue">Hex representation of encrypted value</param>
        /// <returns>Clear/descrypted string</returns>
        public string Decrypt(string encValue)
        {
            Guard.CheckNullOrTrimEmpty(encValue, "Decrypt(encValue)");

            byte[] salt = null;
            byte[] encBytes = null;
            var encParts = encValue.Split('$');

            // Case 1 & 2 supports backward compatibilty for parsing old encrypted strings
            switch (encParts.Length)
            {
                case 1:
                    encBytes = encParts[0].HexToBytes();
                    break;
                case 2:
                    salt = encParts[0].HexToBytes();
                    encBytes = encParts[1].HexToBytes();
                    break;
                case 3:
                    if (encParts[0] != this.CipherAlgo.ToString())
                        throw new InvalidOperationException(string.Format("Encrypted data algorigthm '{0}' doesn't match with Cipher alogrithm '{1}'", encParts[0], this.CipherAlgo));

                    salt = encParts[1].HexToBytes();
                    encBytes = encParts[2].HexToBytes();
                    break;
                default:
                    throw new ArgumentException("encValue is not valid, it should contain encryption algorithm, salt and encryption string seperated by '$' e.g 'AES$F8F25518$23C1916FF7C0A35166BEBCE564D19586'");
            }
            
            if (salt != null && salt.Length > 0)
            {
                using (ICryptoTransform cTransform = encrypto.CreateDecryptor(encrypto.Key, salt))
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(encBytes, 0, encBytes.Length);
                    return UTF8Encoding.Unicode.GetString(resultArray);
                }
            }

            using (ICryptoTransform cTransform = encrypto.CreateDecryptor())
            {
                byte[] resultArray = cTransform.TransformFinalBlock(encBytes, 0, encBytes.Length);
                return UTF8Encoding.Unicode.GetString(resultArray);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.encrypto != null)
                {
                    this.encrypto.Dispose();
                    this.encrypto = null;
                }
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
