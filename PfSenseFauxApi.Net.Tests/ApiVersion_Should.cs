using System;
using Xunit;
using Xunit.Abstractions;

namespace PfSenseFauxApi.Net.Tests
{
    public class ApiVersion_Should
    {
        private readonly Device            _dev;
        private readonly ITestOutputHelper _output;

        public ApiVersion_Should(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException();
            _dev    = new TestConfig().GetDevice();
        }

        [Fact]
        public void BeSet()
        {
            _output.WriteLine(_dev.ApiVersion?.ToString() ?? "Not Set");
            Assert.NotNull(_dev.ApiVersion);
            Assert.Equal(1, _dev.ApiVersion.Major);
            Assert.True(_dev.ApiVersion.Minor >= 30);
        }
        
    }
}
