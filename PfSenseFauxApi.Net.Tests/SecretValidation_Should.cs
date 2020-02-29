using System.Runtime.InteropServices;
using Xunit;

namespace PfSenseFauxApi.Net.Tests
{
    public class SecretValidation_Should
    {
        [Theory]
        [InlineData("0123456789012345678901234567890123456789")]
        [InlineData("01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567")]
        [InlineData("01234567890123456789abcdef01234567890123456789ABCDEF")]
        public void PassValidSecrets(string secret)
        {
            Assert.Matches(Device.ValidSecret, secret);
        }

        [Theory]
        [InlineData("1234")]                                                                                                                              // too short
        [InlineData("012345678901234567890123456789012345678")]                                                                                           // too short
        [InlineData("01234567890123456789-01234567890123456789")]                                                                                         // non-alphanum
        [InlineData("012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678")] // too long
        public void FailInvalidSecrets(string secret)
        {
            Assert.DoesNotMatch(Device.ValidSecret, secret);
        }
    }
}
