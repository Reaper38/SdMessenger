using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Sdm.Core.Util;

namespace Sdm.Core.Crypto.Detail
{
    internal class AESCryptoProviderNET : ISymmetricCryptoProvider
    {
        private Aes aes = Aes.Create();
        private List<int> validKeySizes = null;
        private bool disposed = false;
        
        #region ISymmetricCryptoProvider Members

        public SdmSymmetricAlgorithm Algorithm { get { return SdmSymmetricAlgorithm.AES; } }

        public byte[] Key
        {
            get { return aes.Key; }
            set { aes.Key = value; }
        }

        public byte[] IV
        {
            get { return aes.IV; }
            set { aes.IV = value; }
        }

        private static void Transform(ICryptoTransform transform, Stream dst, Stream src, int srcByteCount)
        {
            using (var cs = new CryptoStream(dst, transform, CryptoStreamMode.Write))
            {
                src.ReadTo(cs, srcByteCount);
            }
        }

        public void Encrypt(Stream dst, Stream src, int srcByteCount)
        {
            using (var transform = aes.CreateEncryptor())
            {
                Transform(transform, dst, src, srcByteCount);
            }
        }

        public void Decrypt(Stream dst, Stream src, int srcByteCount)
        {
            using (var transform = aes.CreateDecryptor())
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
                    var keySizes = aes.LegalKeySizes;
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

        public int KeySize
        {
            set { aes.KeySize = value; }
            get { return aes.KeySize; }
        }

        public int ComputeEncryptedSize(int noncryptedSize)
        {
            var blockSize = aes.BlockSize / 8;
            var blocks = noncryptedSize / blockSize;
            if (noncryptedSize % blockSize != 0)
                blocks++;
            return blocks * blockSize;
        }

        public int ComputeDecryptedSize(int encryptedSize)
        { return encryptedSize; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    aes.Dispose();
                DisposeHelper.OnDispose<AESCryptoProviderNET>(disposing);
                disposed = true;
            }
        }

        ~AESCryptoProviderNET() { Dispose(false); }

        #endregion
    }
}
