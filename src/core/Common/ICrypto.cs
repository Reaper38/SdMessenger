namespace Sdm.Core.Common
{
    public interface ICrypto
    {
        public string GetPrivateKeyASym();
        public string GetPublicKeyASym();
        public string EncryptASym(string plaintext, string publicKey_ = null);
        public string DecryptASym(string ciphertext, string privateKey_ = null);
        public string EncryptSym(string input, string password);
        public string DecryptSym(string input, string password);
        
    }
}
