using System;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PfSenseFauxApi.Net.Tests
{
    public class ConfigBackup_Should
    {
        private readonly ApiContext _dev;
        private readonly ITestOutputHelper _output;

        public ConfigBackup_Should(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException();
            _dev = new TestConfig().GetApiContext();
        }

        [Fact]
        public void ReturnValue()
        {
            var data = _dev.ConfigBackup();
            
            Assert.NotNull(data);

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            
            _output.WriteLine(json);

        }
    }
}
