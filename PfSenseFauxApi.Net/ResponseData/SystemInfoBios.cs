using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class SystemInfoBios
    {
        [JsonPropertyName("vendor")]
        public string Vendor { get; set; }
        
        [JsonPropertyName("version")]
        public string Version { get; set; }
        
        [JsonPropertyName("date")]
        public string Date { get; set; }
    }
}
