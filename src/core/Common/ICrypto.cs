namespace Sdm.Core.Common
{
    public interface ICrypto
    {
        string GetPrivateKeyASym();
        string GetPublicKeyASym();
        string EncryptASym(string plaintext, string publicKey_ = null);
        string DecryptASym(string ciphertext, string privateKey_ = null);
        string EncryptSym(string input, string password);
        string DecryptSym(string input, string password);

    }
}
