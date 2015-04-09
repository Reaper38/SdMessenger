namespace Sdm.Core
{
    public interface ICrypto
    {
        byte[] GetPrivateKeyASym();
        byte[] GetPublicKeyASym();
        string EncryptASym(string plaintext, string publicKey_ = null);
        string DecryptASym(string ciphertext, string privateKey_ = null);
        string EncryptSym(string input, string password, byte[] saltBytes);
        string DecryptSym(string input, string password, byte[] saltBytes);

    }
}
