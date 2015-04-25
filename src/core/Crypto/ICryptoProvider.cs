using System;
using System.Collections.Generic;
using System.IO;

namespace Sdm.Core
{
    public interface ICryptoProvider : IDisposable
    {
        /// <summary>
        /// Gets valid key sizes (in bits).
        /// </summary>
        IEnumerable<int> ValidKeySizes { get; }
        /// <summary>
        /// Gets or sets the size (in bits) of the key used by the underlying crypto algorithm.
        /// </summary>
        int KeySize { get; set; }
        /// <summary>
        /// Returns size of encrypted data.
        /// </summary>
        /// <param name="noncryptedSize">Size of reference (non-crypted) data.</param>
        int ComputeEncryptedSize(int noncryptedSize);
        /// <summary>
        /// Returns size of decrypted data.
        /// </summary>
        /// <param name="encryptedSize">Size of encrypted data.</param>
        int ComputeDecryptedSize(int encryptedSize);
    }

    public enum SdmSymmetricAlgorithm
    {
        AES,
        Blowfish,
    }

    public enum SdmAsymmetricAlgorithm
    {
        RSA,
        RSACrypto,
    }

    public interface ISymmetricCryptoProvider : ICryptoProvider
    {
        SdmSymmetricAlgorithm Algorithm { get; }
        /// <summary>
        /// Gets or sets the secret key for the symmetric algorithm.
        /// </summary>
        byte[] Key { get; set; }
        /// <summary>
        /// Gets or sets the initialization vector (IV) for the symmetric algorithm.
        /// </summary>
        byte[] IV { get; set; }
        void Encrypt(Stream dst, Stream src, int srcByteCount);
        void Decrypt(Stream dst, Stream src, int srcByteCount);
    }
    
    public interface IAsymmetricCryptoProvider : ICryptoProvider
    {
        SdmAsymmetricAlgorithm Algorithm { get; }
        string GetKey(bool includePrivateParams = false);
        void SetKey(string key);
        byte[] Encrypt(byte[] src);
        byte[] Decrypt(byte[] src);
    }
}
