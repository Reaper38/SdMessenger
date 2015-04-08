using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Sdm.Core
{
    class Crypto_RSA
    {
        readonly RSACryptoServiceProvider _rsa = new RSACryptoServiceProvider(1024);
        readonly string _privateKey;
        readonly string _publicKey;

        public Crypto_RSA()
        {
            _privateKey = _rsa.ToXmlString(true);
            _publicKey = _rsa.ToXmlString(false);
        }

        public byte[] GetPrivateKey()
        {
            return Encoding.Unicode.GetBytes(_privateKey);
        }

        public byte[] GetPublicKey()
        {
            return Encoding.Unicode.GetBytes(_publicKey);
        }
        public string DecryptASym(string ciphertext, string privateKey_ = null)
        {
            if (ciphertext.Length <= 0) throw new ArgumentNullException("ciphertext");

            string key = String.IsNullOrEmpty(privateKey_) ? _privateKey : privateKey_;
            return DecryptToBytes(ciphertext, key);
        }
        private string DecryptToBytes(string ciphertext, string privateKey)
        {
            if (String.IsNullOrEmpty(privateKey)) throw new ArgumentNullException("privateKey");

            byte[] ciphertext_Bytes = Convert.FromBase64String(ciphertext);
            _rsa.FromXmlString(privateKey);

            byte[] plaintext = _rsa.Decrypt(ciphertext_Bytes, false);
            return Encoding.Unicode.GetString(plaintext);
        }

        public string EncryptASym(string plaintext, string publicKey_ = null)
        {
            if (plaintext.Length <= 0) throw new ArgumentNullException("plaintext");

            string key = String.IsNullOrEmpty(publicKey_) ? _publicKey : publicKey_;
            return EncryptToBytes(plaintext, key);
        }
        private string EncryptToBytes(string plaintext, string publicKey)
        {
            if (String.IsNullOrEmpty(publicKey)) throw new ArgumentNullException("publicKey");

            byte[] plaintext_Bytes = Encoding.Unicode.GetBytes(plaintext);
            _rsa.FromXmlString(publicKey);

            byte[] ciphertext = _rsa.Encrypt(plaintext_Bytes, false);
            return Convert.ToBase64String(ciphertext);
        }
    }
}
