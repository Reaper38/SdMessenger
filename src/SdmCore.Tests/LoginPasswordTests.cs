using System;
using NUnit.Framework;
using Sdm.Core.Util;

namespace SdmCore.Tests
{
    [TestFixture]
    public class LoginPasswordTests
    {
        [Test]
        public void TestLoginValidation()
        {
            // arrange
            const int minLength = 2;
            const int maxLength = 30;
            var validLogins = new[]
            {
                new String('a', minLength), // ==min
                new String('a', maxLength), // ==max
                new String('a', maxLength) + " ", // ==max with whitespace
                "a.0", // single period
                "a.0.z", // two periods
            };
            var invalidLogins = new[]
            {
                "", // empty
                "  ", // whitespace
                "a", // < min
                new String('a', maxLength + 1), // > max
                ".a", // first char is period
                "a.", // last char is period
                "a_a", // invalid char
                "a..a", // consecutive periods
            };
            // act & assert
            string msg;
            for (int i = 0; i < validLogins.Length; i++)
                Assert.IsTrue(NetUtil.ValidateLogin(ref validLogins[i], out msg));
            for (int i = 0; i < invalidLogins.Length; i++)
                Assert.IsFalse(NetUtil.ValidateLogin(ref invalidLogins[i], out msg));
        }

        [Test]
        public void TestPasswordValidation()
        {
            // arrange
            const int minLength = 6;
            const int maxLength = 100;
            var validPasswords = new[]
            {
                new String('a', minLength), // ==min
                new String('a', maxLength), // ==max
                new String('a', maxLength) + " ", // ==max with whitespace
            };
            var invalidPasswords = new[]
            {
                "", // empty
                "  ", // whitespace
                "a", // < min
                new String('a', maxLength + 1), // > max
            };
            // act & assert
            string msg;
            for (int i = 0; i < validPasswords.Length; i++)
                Assert.IsTrue(NetUtil.ValidatePassword(ref validPasswords[i], out msg));
            for (int i = 0; i < invalidPasswords.Length; i++)
                Assert.IsFalse(NetUtil.ValidatePassword(ref invalidPasswords[i], out msg));
        }
    }
}
