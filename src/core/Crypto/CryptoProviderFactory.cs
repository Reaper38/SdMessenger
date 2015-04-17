using System;

namespace Sdm.Core
{
    public sealed class CryptoProviderFactory
    {
        #region Singleton implementation

        private CryptoProviderFactory() {}
        private static readonly CryptoProviderFactory instance = new CryptoProviderFactory();
        public static CryptoProviderFactory Instance { get { return instance; } }

        #endregion

        public ISymmetricCryptoProvider CreateSymmetric(SdmSymmetricAlgorithm algorithm)
        {
            switch (algorithm)
            {
            case SdmSymmetricAlgorithm.AES: return new AESCryptoProvider();
            default: throw new NotSupportedException(algorithm + " is not supported.");
            }
        }

        public IAsymmetricCryptoProvider CreateAsymmetric(SdmAsymmetricAlgorithm algorithm)
        {
            switch (algorithm)
            {
            case SdmAsymmetricAlgorithm.RSA: return new RSACryptoProvider();
            default: throw new NotSupportedException(algorithm + " is not supported.");
            }
        }
    }
}
