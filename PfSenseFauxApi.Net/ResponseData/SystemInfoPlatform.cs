using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class SystemInfoPlatform
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("descr")]
        public string Description { get; set; }
    }
}
