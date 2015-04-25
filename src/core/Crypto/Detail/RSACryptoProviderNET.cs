using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Sdm.Core.Util;

namespace Sdm.Core.Crypto.Detail
{
    internal class RSACryptoProviderNET : IAsymmetricCryptoProvider
    {
        private RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        private List<int> validKeySizes = null;
        private bool disposed = false;

        #region IAsymmetricCryptoProvider Members

        public SdmAsymmetricAlgorithm Algorithm { get { return SdmAsymmetricAlgorithm.RSA; } }

        public string GetKey(bool includePrivateParams = false)
        { return rsa.ToXmlString(includePrivateParams); }

        public void SetKey(string key) { rsa.FromXmlString(key); }

        public byte[] Encrypt(byte[] src)
        { return rsa.Encrypt(src, false); }

        public byte[] Decrypt(byte[] src)
        { return rsa.Decrypt(src, false); }

        #endregion
        
        #region ICryptoProvider Members

        public IEnumerable<int> ValidKeySizes
        {
            get
            {
                if (validKeySizes == null)
                {
                    validKeySizes = new List<int>();
                    var keySizes = rsa.LegalKeySizes;
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
            get { return rsa.KeySize; }
            set
            {
                if (value == rsa.KeySize)
                    return;
                rsa.Dispose();
                rsa = new RSACryptoServiceProvider(value);
            }
        }

        public int ComputeEncryptedSize(int noncryptedSize)
        { return rsa.KeySize / 8; }

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
                    rsa.Dispose();
                DisposeHelper.OnDispose<RSACryptoProviderNET>(disposing);
                disposed = true;
            }
        }

        ~RSACryptoProviderNET() { Dispose(false); }

        #endregion
    }
}
