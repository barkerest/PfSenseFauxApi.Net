using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.Tests
{
    public class TestConfig
    {
        public string Host { get; }
        public string Key { get;  }
        public string Secret { get; }
        
        public string Path => $"https://{Host}/fauxapi/v1/";

        public ApiContext GetApiContext()
        {
            var a = new AuthorizationKey(Key, Secret);
            return new ApiContext(Path, a, false);
        }
        
        private static string ConfigPath([CallerFilePath] string sourceFilePath = "")
        {
            return System.IO.Path.GetDirectoryName(sourceFilePath).TrimEnd('\\', '/') + "/appsettings.json";
        }

        public TestConfig()
        {
            var file = ConfigPath();

            var contents = System.IO.File.ReadAllText(file);
            var doc      = JsonDocument.Parse(contents).RootElement;

            if (doc.TryGetProperty("FauxAPI", out var cfg))
            {
                if (cfg.TryGetProperty("Host", out var value))
                {
                    Host = value.GetString();
                }

                if (cfg.TryGetProperty("Key", out value))
                {
                    Key = value.GetString();
                }

                if (cfg.TryGetProperty("Secret", out value))
                {
                    Secret = value.GetString();
                }
            }

            if (string.IsNullOrEmpty(Host))
            {
                throw new ArgumentException("Missing host.");
            }
                    
            if (!AuthorizationKey.ValidKey.IsMatch(Key))
            {
                throw new ArgumentException("Missing valid key.");
            }

            if (!AuthorizationKey.ValidSecret.IsMatch(Secret))
            {
                throw new ArgumentException("Missing valid secret.");
            }
        }
    }
}
