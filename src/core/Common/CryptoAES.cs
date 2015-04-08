using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Sdm.Core.Common
{
    /// <summary>Represents metods for AES encoding</summary>
    public class CryptoAES
    {
        private static readonly uint SaltLenght = 7;
        public readonly byte[] saltBytes = new byte[SaltLenght];   
   
        CryptoAES()
        {
            Random rnd = new Random();
            rnd.NextBytes(saltBytes);
        }

        /// <summary>Metod for encryption of strings with AES</summary>
        public string EncryptSym(string input, string password, byte[] saltBytes)
        {
            // Get the bytes of the string
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] bytesEncrypted = EncryptAES(bytesToBeEncrypted, passwordBytes, saltBytes);
            string result = Convert.ToBase64String(bytesEncrypted);
            return result;
        }

        private byte[] EncryptAES(byte[] bytesToBeEncrypted, byte[] passwordBytes, byte[] saltBytes)
        {
            byte[] encryptedBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }

        /// <summary>Metod for decryption AES encoded strings</summary>
        public string DecryptSym(string input, string password, byte[] saltBytes)
        {
            if (saltBytes.Length < 7) throw new ArgumentException("DecryptSym: Salt is too short");
            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = DecryptAES(bytesToBeDecrypted, passwordBytes, saltBytes);
            string result = Encoding.UTF8.GetString(bytesDecrypted);
            return result;
        }

        private byte[] DecryptAES(byte[] bytesToBeDecrypted, byte[] passwordBytes, byte[] saltBytes)
        {
            byte[] decryptedBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }
            return decryptedBytes;
        }
    }
}


