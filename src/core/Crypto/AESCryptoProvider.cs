using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Sdm.Core
{
    public class AESCryptoProvider : ISymmetricCryptoProvider
    {
        private RijndaelManaged rij = new RijndaelManaged { BlockSize = 128, Mode = CipherMode.CBC };
        private List<int> validKeySizes = null;
        
        #region ISymmetricCryptoProvider Members

        public byte[] Key
        {
            get { return rij.Key; }
            set { rij.Key = value; }
        }

        public byte[] IV
        {
            get { return rij.IV; }
            set { rij.IV = value; }
        }

        private static void Transform(ICryptoTransform transform, Stream dst, Stream src, int srcByteCount)
        {
            using (var cs = new CryptoStream(dst, transform, CryptoStreamMode.Write))
            {
                var buf = new byte[256];
                var readByteCount = 0;
                while (readByteCount < srcByteCount)
                {
                    var r = src.Read(buf, 0, Math.Min(buf.Length, srcByteCount - readByteCount));
                    cs.Write(buf, 0, r);
                    readByteCount += r;
                }
            }
        }

        public void Encrypt(Stream dst, Stream src, int srcByteCount)
        {
            using (var transform = rij.CreateEncryptor())
            {
                Transform(transform, dst, src, srcByteCount);
            }
        }

        public void Decrypt(Stream dst, Stream src, int srcByteCount)
        {
            using (var transform = rij.CreateDecryptor())
            {
                Transform(transform, dst, src, srcByteCount);
            }
        }

        #endregion

        #region ICryptoProvider Members

        public IEnumerable<int> ValidKeySizes
        {
            get
            {
                if (validKeySizes == null)
                {
                    validKeySizes = new List<int>();
                    var keySizes = rij.LegalKeySizes;
                    for (int i = 0; i < keySizes.Length; i++)
                    {
                        if (keySizes[i].SkipSize == 0)
                            validKeySizes.Add(keySizes[i].MinSize);
                        else
                        {
                            for (int j = keySizes[i].MinSize;
                                j <= keySizes[i].MaxSize;
                                j += keySizes[i].SkipSize)
                            {
                                validKeySizes.Add(j);
                            }
                        }
                    }
                }
                return validKeySizes.AsReadOnly();
            }
        }

        public void Initialize(int keySize)
        { rij.KeySize = keySize; }

        public int ComputeEncryptedSize(int noncryptedSize)
        {
            var blockSize = rij.BlockSize / 8;
            var blocks = noncryptedSize / blockSize;
            if (noncryptedSize % blockSize != 0)
                blocks++;
            return blocks * blockSize;
        }

        public int ComputeDecryptedSize(int encryptedSize)
        { return encryptedSize; }

        #endregion
    }
}
