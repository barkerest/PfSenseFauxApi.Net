using Xunit;

namespace PfSenseFauxApi.Net.Tests
{
    public class KeyValidation_Should
    {
        [Theory]
        [InlineData("PFFAValidItem1")]
        [InlineData("PFFAexample03")]
        [InlineData("PFFA0123456789abcdef")]
        [InlineData("PFFA0123456789abcdef0123456789abcdef0123")]
        public void PassValidKeys(string key)
        {
            Assert.Matches(Device.ValidKey, key);
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
            Assert.DoesNotMatch(Device.ValidKey, key);
        }
    }
}
