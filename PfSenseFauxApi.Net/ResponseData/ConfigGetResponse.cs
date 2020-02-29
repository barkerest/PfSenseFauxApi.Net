using System.Text.Json;
using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class ConfigGetResponse
    {
        [JsonPropertyName("config_file")]
        public string ConfigFile { get; set; }
        
        [JsonPropertyName("config")]
        public JsonElement Config { get; set; }
    }
}
