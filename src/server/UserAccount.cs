using System;
using System.Security.Cryptography;
using System.Text;
using Sdm.Core;

namespace Sdm.Server
{
    public sealed class UserAccount
    {
        private static SHA256 sha = SHA256.Create(); // XXX: dispose?
        private const string PasswordSalt = "zx$$";

        public string Login { get; private set; }
        /// <summary>Base64 string with SHA-256 hash of password+salt</summary>
        public string Password { get; private set; }
        public UserAccess Access { get; set; }

        public UserAccount(string login, string passwordHash, UserAccess access)
        {
            Login = login;
            Password = passwordHash;
            Access = access;
        }

        public static string TransformPassword(string plainPassword)
        {
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(plainPassword + PasswordSalt));
            var shash = Convert.ToBase64String(hash);
            return shash;
        }

        public bool VerifyPassword(string plainPassword)
        {
            var shash = TransformPassword(plainPassword);
            return shash == Password;
        }
    }
}
