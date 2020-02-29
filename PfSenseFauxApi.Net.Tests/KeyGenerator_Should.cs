using Xunit;

namespace PfSenseFauxApi.Net.Tests
{
    public class KeyGenerator_Should
    {
        [Fact]
        public void GenerateValidKeySecretPairs()
        {
            for (var i = 0; i < 100; i++)
            {
                var (key, secret) = Device.GenerateKeyPair();
                Assert.Matches(Device.ValidKey, key);
                Assert.Matches(Device.ValidSecret, secret);
            }
        }
    }
}
