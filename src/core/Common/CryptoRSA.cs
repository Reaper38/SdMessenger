using System;
using System.Text;
using System.Security.Cryptography;

namespace Sdm.Core
{
    /// <summary>Represents metods for RSA encoding</summary>
    public class CryptoRSA
    {
        private readonly static int RSAKeyLenght = 2048;
        private readonly RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(RSAKeyLenght);
        public readonly string privateKey;      /// <summary>RSA private key</summary>
        public readonly string publicKey;       /// <summary>RSA public key</summary>

        public CryptoRSA()
        {           
            privateKey = rsa.ToXmlString(true);
            publicKey = rsa.ToXmlString(false);
        }
        /// <summary>Metod for decryption RSA encoded strings</summary>
        public string DecryptASym(string ciphertext, string privateKey_ = null)
        {
            if (ciphertext.Length <= 0) throw new ArgumentNullException("ciphertext");

            string key = String.IsNullOrEmpty(privateKey_) ? privateKey : privateKey_;
            return DecryptRSA(ciphertext, key);
        }
        private string DecryptRSA(string ciphertext, string privateKey)
        {
            if (String.IsNullOrEmpty(privateKey)) throw new ArgumentNullException("privateKey");

            byte[] ciphertext_Bytes = Convert.FromBase64String(ciphertext);
            rsa.FromXmlString(privateKey);

            byte[] plaintext = rsa.Decrypt(ciphertext_Bytes, false);
            return Encoding.Unicode.GetString(plaintext);
        }

        /// <summary>Metod for encryption of strings with RSA</summary>
        public string EncryptASym(string plaintext, string publicKey_ = null)
        {
            if (plaintext.Length <= 0) throw new ArgumentNullException("plaintext");

            string key = String.IsNullOrEmpty(publicKey_) ? publicKey : publicKey_;
            return EncryptRSA(plaintext, key);
        }
        private string EncryptRSA(string plaintext, string publicKey)
        {
            if (String.IsNullOrEmpty(publicKey)) throw new ArgumentNullException("publicKey");

            byte[] plaintext_Bytes = Encoding.Unicode.GetBytes(plaintext);
            rsa.FromXmlString(publicKey);

            byte[] ciphertext = rsa.Encrypt(plaintext_Bytes, false);
            return Convert.ToBase64String(ciphertext);
        }
    }
}
