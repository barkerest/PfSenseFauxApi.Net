using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class SystemInfoPfSenseVersion
    {
        [JsonPropertyName("product_version_string")]
        public string ProductVersionString { get; set; }
        
        [JsonPropertyName("product_version")]
        public string ProductVersion { get; set; }
        
        [JsonPropertyName("product_version_patch")]
        public string ProductVersionPatch { get; set; }
    }
}
