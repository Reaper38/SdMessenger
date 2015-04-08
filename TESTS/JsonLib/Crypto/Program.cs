using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;


namespace Crypto
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

        public string GetPrivateKey()
        {
            return _privateKey;
        }

        public string GetPublicKey()
        {
            return _publicKey;
        }
        public string Decrypt(string ciphertext, string privateKey_ = null)
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

        public string Encrypt(string plaintext, string publicKey_ = null)
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
    class Program
    {
        private static void Keep_Open()
        {
            Console.Read();
        }
        static void Main(string[] args)
        {
            Crypto_RSA cry = new Crypto_RSA();
            string priv = cry.GetPrivateKey();
            string pub  = cry.GetPublicKey();
            string enc = "";
            string dec = "";
            Console.WriteLine("private\n" + priv + "\npublic\n" + pub + "\n");
            enc = cry.Encrypt("asdasdavgsgsdbfsDbfFDSbfSDFBfASBFASB", pub.ToString());
            Console.WriteLine("Encrypted\n" + enc + "\n");
            Crypto_RSA cry2 = new Crypto_RSA();
            dec = cry2.Decrypt(enc, priv);
            Console.WriteLine("\nDecrypted\n" + dec);
            Keep_Open();
        }
    }
}

