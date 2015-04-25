using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Sdm.Core.Util;
using CryptoNet;

namespace Sdm.Core.Crypto.Detail
{
    internal class BlowfishCryptoProvider : ISymmetricCryptoProvider
    {
        private Blowfish blow = new Blowfish { BlockSize = 128, Mode = CipherMode.CBC };
        private List<int> validKeySizes = null;
        private bool disposed = false;

        #region ISymmetricCryptoProvider Members

        public SdmSymmetricAlgorithm Algorithm { get { return SdmSymmetricAlgorithm.AES; } }

        public byte[] Key
        {
            get { return blow.Key; }
            set { blow.Key = value; }
        }

        public byte[] IV
        {
            get { return blow.IV; }
            set { blow.IV = value; }
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
            using (var transform = blow.CreateEncryptor())
            {
                Transform(transform, dst, src, srcByteCount);
            }
        }

        public void Decrypt(Stream dst, Stream src, int srcByteCount)
        {
            using (var transform = blow.CreateDecryptor())
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
                    var keySizes = blow.LegalKeySizes;
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
            set { blow.KeySize = value; }
            get { return blow.KeySize; }
        }

        public int ComputeEncryptedSize(int noncryptedSize)
        {
            var blockSize = blow.BlockSize / 8;
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
                    blow.Dispose();
                DisposeHelper.OnDispose<BlowfishCryptoProvider>(disposing);
                disposed = true;
            }
        }

        ~BlowfishCryptoProvider() { Dispose(false); }

        #endregion
    }
}
