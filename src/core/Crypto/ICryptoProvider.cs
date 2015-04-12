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
        /// Initializes ICryptoProvider instance using given key size.
        /// </summary>
        /// <param name="keySize">Key size (in bits)</param>
        void Initialize(int keySize);
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

    public interface ISymmetricCryptoProvider : ICryptoProvider
    {
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
        string GetKey(bool includePrivateParams = false);
        void SetKey(string key);
        byte[] Encrypt(byte[] src);
        byte[] Decrypt(byte[] src);
    }
}
