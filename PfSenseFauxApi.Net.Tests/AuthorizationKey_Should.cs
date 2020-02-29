using System;
using Xunit;

namespace PfSenseFauxApi.Net.Tests
{
    public class AuthorizationKey_Should
    {
        private const string KnownGoodKey = "PFFAValidItem1";
        private const string KnownGoodSecret = "0123456789012345678901234567890123456789";
        private const string KnownGoodToken = "PFFAValidItem1:20200229Z181318:51ee0fbb:e2b4b69603d064f90e134d49d0f872f263e7666a51a7d8b9553e51ffaba7dcc4";
        
        [Theory]
        [InlineData(KnownGoodKey)]
        [InlineData("PFFAexample03")]
        [InlineData("PFFA0123456789abcdef")]
        [InlineData("PFFA0123456789abcdef0123456789abcdef0123")]
        public void PassValidKeys(string key)
        {
            Assert.Matches(AuthorizationKey.ValidKey, key);
            var a = new AuthorizationKey(key, KnownGoodSecret);
            Assert.NotNull(a);
        }

        [Theory]
        [InlineData("pffainvaliditem")]                           // lower case pffa
        [InlineData("PFFAexample01")]                             // prohibited
        [InlineData("PFFAexample02")]                             // prohibited
        [InlineData("PFFA1234")]                                  // too short
        [InlineData("PFFA1234567")]                               // too short
        [InlineData("PFFA-example99")]                            // non-alphanum
        [InlineData("PFFA0123456789abcdef0123456789abcdef01234")] // too long
        [InlineData("abcdefghijklmnop")]                          // no PFFA
        public void FailsInvalidKeys(string key)
        {
            Assert.DoesNotMatch(AuthorizationKey.ValidKey, key);
            var x = Assert.Throws<ArgumentException>(
                () =>
                {
                    var a = new AuthorizationKey(key, KnownGoodSecret);
                }
            );
            Assert.Equal("key", x.ParamName);
        }
        
        [Theory]
        [InlineData(KnownGoodSecret)]
        [InlineData("01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567")]
        [InlineData("01234567890123456789abcdef01234567890123456789ABCDEF")]
        public void PassValidSecrets(string secret)
        {
            Assert.Matches(AuthorizationKey.ValidSecret, secret);
            var a = new AuthorizationKey(KnownGoodKey, secret);
            Assert.NotNull(a);
        }

        [Theory]
        [InlineData("1234")]                                                                                                                              // too short
        [InlineData("012345678901234567890123456789012345678")]                                                                                           // too short
        [InlineData("01234567890123456789-01234567890123456789")]                                                                                         // non-alphanum
        [InlineData("012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678")] // too long
        public void FailInvalidSecrets(string secret)
        {
            Assert.DoesNotMatch(AuthorizationKey.ValidSecret, secret);
            var x = Assert.Throws<ArgumentException>(
                () =>
                {
                    var a = new AuthorizationKey(KnownGoodKey, secret);
                }
            );
            Assert.Equal("secret", x.ParamName);
        }

        [Fact]
        public void GenerateNewKeys()
        {
            var lastKey = "";
            var lastSecret = "";
            for (var i = 0; i < 100; i++)
            {
                var a = new AuthorizationKey();
                Assert.False(string.IsNullOrEmpty(a.Key));
                Assert.False(string.IsNullOrEmpty(a.Secret));
                Assert.Matches(AuthorizationKey.ValidKey, a.Key);
                Assert.Matches(AuthorizationKey.ValidSecret, a.Secret);
                
                // theoretically possible that these will fail, but very unlikely.
                Assert.NotEqual(lastKey, a.Key);
                Assert.NotEqual(lastSecret, a.Secret);
                lastKey = a.Key;
                lastSecret = a.Secret;
            }
        }

        [Fact]
        public void ValidatesKnownGoodToken()
        {
            var a = new AuthorizationKey(KnownGoodKey, KnownGoodSecret);
            Assert.True(a.ValidateToken(KnownGoodToken));
        }

        [Fact]
        public void DoesNotValidateBrokenToken()
        {
            var a = new AuthorizationKey(KnownGoodKey, KnownGoodSecret);
            var token = KnownGoodToken.Replace("Item1", "Item2");
            Assert.False(a.ValidateToken(token));
        }

        [Fact]
        public void GenerateToken()
        {
            var a = new AuthorizationKey(KnownGoodKey, KnownGoodSecret);
            var token = a.GenerateToken();
            Assert.False(string.IsNullOrEmpty(token));
            Assert.NotEqual(KnownGoodToken, token);    // the generated token must not match the known good token.
            Assert.True(a.ValidateToken(token));
        }
    }
}
