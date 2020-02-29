using System;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PfSenseFauxApi.Net.Tests
{
    public class SystemStats_Should
    {
        private readonly Device _dev;
        private readonly ITestOutputHelper _output;

        public SystemStats_Should(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException();
            _dev = TestConfig.Instance.GetDevice();
        }

        [Fact]
        public void ReturnValue()
        {
            var data = _dev.SystemStats();
            
            Assert.NotNull(data);

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            
            _output.WriteLine(json);

        }
    }
}
