using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.Tests
{
    public class TestConfig
    {
        public string Host { get; private set; }
        public string Key { get; private set; }
        public string Secret { get; private set; }
        
        public string Path => $"https://{Host}/fauxapi/v1/";

        public Device GetDevice()
        {
            return new Device(Path, Key, Secret, false);
        }
        
        private static string ConfigPath([CallerFilePath] string sourceFilePath = "")
        {
            return System.IO.Path.GetDirectoryName(sourceFilePath).TrimEnd('\\', '/') + "/appsettings.json";
        }

        private TestConfig()
        {
            
        }
        
        private static TestConfig _instance;
        private static readonly object Tlock = new object();
        
        public static TestConfig Instance
        {
            get
            {
                lock (Tlock)
                {
                    if (_instance != null) return _instance;

                    var ret = new TestConfig();

                    var file = ConfigPath();

                    var contents = System.IO.File.ReadAllText(file);
                    var doc      = JsonDocument.Parse(contents).RootElement;

                    if (doc.TryGetProperty("FauxAPI", out var cfg))
                    {
                        if (cfg.TryGetProperty("Host", out var value))
                        {
                            ret.Host = value.GetString();
                        }

                        if (cfg.TryGetProperty("Key", out value))
                        {
                            ret.Key = value.GetString();
                        }

                        if (cfg.TryGetProperty("Secret", out value))
                        {
                            ret.Secret = value.GetString();
                        }
                    }

                    if (string.IsNullOrEmpty(ret.Host))
                    {
                        throw new ArgumentException("Missing host.");
                    }
                    
                    if (!Device.ValidKey.IsMatch(ret.Key))
                    {
                         throw new ArgumentException("Missing valid key.");
                    }

                    if (!Device.ValidSecret.IsMatch(ret.Secret))
                    {
                        throw new ArgumentException("Missing valid secret.");
                    }


                    _instance = ret;
                }

                return _instance;
            }
        }
    }
}
