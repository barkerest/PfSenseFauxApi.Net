using System;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PfSenseFauxApi.Net.Tests
{
    public class SystemInfo_Should
    {
        private readonly Device _dev;
        private readonly ITestOutputHelper _output;

        public SystemInfo_Should(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException();
            _dev = new TestConfig().GetDevice();
        }

        [Fact]
        public void ReturnValue()
        {
            var data = _dev.SystemInfo();
            
            Assert.NotNull(data);

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            
            _output.WriteLine(json);

        }
    }
}
