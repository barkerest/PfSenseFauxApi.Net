using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class ConfigRestoreResponse
    {
        [JsonPropertyName("config_file")]
        public string ConfigFile { get; set; }
    }
}
