using System;
using Sdm.Core.Crypto.Detail;

namespace Sdm.Core.Crypto
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
            case SdmSymmetricAlgorithm.Blowfish: return new BlowfishCryptoProvider();
            default: throw new NotSupportedException(algorithm + " is not supported.");
            }
        }

        public IAsymmetricCryptoProvider CreateAsymmetric(SdmAsymmetricAlgorithm algorithm)
        {
            switch (algorithm)
            {
            case SdmAsymmetricAlgorithm.RSA: return new RSACryptoProviderNET();
            case SdmAsymmetricAlgorithm.RSACrypto: return new RSACryptoProviderCryptoNET();
            default: throw new NotSupportedException(algorithm + " is not supported.");
            }
        }
    }
}
