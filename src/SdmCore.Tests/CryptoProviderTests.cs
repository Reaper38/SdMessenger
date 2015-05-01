using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using NUnit.Framework;
using Sdm.Core.Crypto.Detail;
using Sdm.Core.Util;

namespace SdmCore.Tests
{
    [TestFixture]
    public class CryptoProviderTests
    {
        private ThreadLocal<RandomNumberGenerator> rng;

        public CryptoProviderTests()
        { rng = new ThreadLocal<RandomNumberGenerator>(() => new RNGCryptoServiceProvider()); }

        private void GenerateKey(byte[] key) { rng.Value.GetBytes(key); }
        
        private static int GetMaxRsaInputSize(int keySize)
        { return ((keySize - 384) / 8) + 37; }

        [Test]
        public void TestRsa()
        {
            // arrange
            using (var csp = new RSACryptoProviderNET())
            {
                var random = new Random(42);
                var keySizes = new[] {384, 1024, 2048};
                // act & assert
                foreach (var sz in keySizes)
                {
                    var maxDataSize = GetMaxRsaInputSize(sz);
                    var shortData = new byte[maxDataSize / 2];
                    var longData = new byte[maxDataSize];
                    random.NextBytes(shortData);
                    random.NextBytes(longData);
                    csp.KeySize = sz;
                    var shortCipher = csp.Encrypt(shortData);
                    var decryptedShortData = csp.Decrypt(shortCipher);
                    Assert.AreEqual(shortData, decryptedShortData);
                    var longCipher = csp.Encrypt(longData);
                    var decryptedLongData = csp.Decrypt(longCipher);
                    Assert.AreEqual(longData, decryptedLongData);
                }
            }
        }

        [Test]
        public void TestRsaCryptoNET()
        {
            // arrange
            using (var csp = new RSACryptoProviderCryptoNET())
            {
                var random = new Random(42);
                var keySizes = new[] {384, 1024, 2048};
                // act & assert
                foreach (var sz in keySizes)
                {
                    var maxDataSize = GetMaxRsaInputSize(sz);
                    var shortData = new byte[maxDataSize / 2];
                    var longData = new byte[maxDataSize];
                    random.NextBytes(shortData);
                    random.NextBytes(longData);
                    csp.KeySize = sz;
                    var shortCipher = csp.Encrypt(shortData);
                    var decryptedShortData = csp.Decrypt(shortCipher);
                    Assert.AreEqual(shortData, decryptedShortData);
                    var longCipher = csp.Encrypt(longData);
                    var decryptedLongData = csp.Decrypt(longCipher);
                    Assert.AreEqual(longData, decryptedLongData);
                }
            }
        }

        [Test]
        public void TestAesDefaultKey()
        {
            // arrange
            using (var csp = new AESCryptoProvider())
            {
                var random = new Random(42);
                var data = new byte[(int)(csp.KeySize * 1.5)];
                random.NextBytes(data);
                var msSrc = new MemoryStream();
                var mswSrc = msSrc.AsUnclosable();
                var msDst = new MemoryStream();
                var mswDst = msDst.AsUnclosable();
                mswSrc.Write(data, 0, data.Length);
                mswSrc.Position = 0;
                // act
                csp.Encrypt(mswDst, mswSrc, (int)mswSrc.Length);
                mswSrc.Position = 0;
                mswDst.Position = 0;
                csp.Decrypt(mswSrc, mswDst, (int)mswDst.Length);
                var decryptedData = msSrc.GetBuffer();
                // assert
                for (int i = 0; i < msSrc.Length; i++)
                    Assert.AreEqual(data[i], decryptedData[i]);
            }
        }

        [Test]
        public void TestAes()
        {
            // arrange
            using (var csp = new AESCryptoProvider())
            {
                var random = new Random(42);
                var keySizes = csp.ValidKeySizes;
                foreach (var sz in keySizes)
                {
                    csp.KeySize = sz;
                    var key = new byte[sz / 8];
                    GenerateKey(key);
                    var data = new byte[(int)(sz / 8 * 1.5)];
                    random.NextBytes(data);
                    var msSrc = new MemoryStream();
                    var mswSrc = msSrc.AsUnclosable();
                    var msDst = new MemoryStream();
                    var mswDst = msDst.AsUnclosable();
                    mswSrc.Write(data, 0, data.Length);
                    mswSrc.Position = 0;
                    // act
                    csp.Key = key;
                    csp.Encrypt(mswDst, mswSrc, (int)mswSrc.Length);
                    mswSrc.Position = 0;
                    mswDst.Position = 0;
                    csp.Decrypt(mswSrc, mswDst, (int)mswDst.Length);
                    var decryptedData = msSrc.GetBuffer();
                    // assert
                    for (int i = 0; i < msSrc.Length; i++)
                        Assert.AreEqual(data[i], decryptedData[i]);
                }
            }
        }

        [Test]
        public void TestBlowfishDefaultKey()
        {
            // arrange
            using (var csp = new BlowfishCryptoProvider())
            {
                var random = new Random(42);
                var data = new byte[(int)(csp.KeySize * 1.5)];
                random.NextBytes(data);
                var msSrc = new MemoryStream();
                var mswSrc = msSrc.AsUnclosable();
                var msDst = new MemoryStream();
                var mswDst = msDst.AsUnclosable();
                mswSrc.Write(data, 0, data.Length);
                mswSrc.Position = 0;
                // act
                csp.Encrypt(mswDst, mswSrc, (int)mswSrc.Length);
                mswSrc.Position = 0;
                mswDst.Position = 0;
                csp.Decrypt(mswSrc, mswDst, (int)mswDst.Length);
                var decryptedData = msSrc.GetBuffer();
                // assert
                for (int i = 0; i < msSrc.Length; i++)
                    Assert.AreEqual(data[i], decryptedData[i]);
            }
        }

        [Test]
        public void TestBlowfish()
        {
            // arrange
            using (var csp = new BlowfishCryptoProvider())
            {
                var random = new Random(42);
                var keySizes = csp.ValidKeySizes;
                foreach (var sz in keySizes)
                {
                    csp.KeySize = sz;
                    var key = new byte[sz / 8];
                    GenerateKey(key);
                    var data = new byte[(int)(sz / 8 * 1.5)];
                    random.NextBytes(data);
                    var msSrc = new MemoryStream();
                    var mswSrc = msSrc.AsUnclosable();
                    var msDst = new MemoryStream();
                    var mswDst = msDst.AsUnclosable();
                    mswSrc.Write(data, 0, data.Length);
                    mswSrc.Position = 0;
                    // act
                    csp.Key = key;
                    csp.Encrypt(mswDst, mswSrc, (int)mswSrc.Length);
                    mswSrc.Position = 0;
                    mswDst.Position = 0;
                    csp.Decrypt(mswSrc, mswDst, (int)mswDst.Length);
                    var decryptedData = msSrc.GetBuffer();
                    // assert
                    for (int i = 0; i < msSrc.Length; i++)
                        Assert.AreEqual(data[i], decryptedData[i]);
                }
            }
        }
    }
}
