using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class SystemInfoSysEntry
    {
        [JsonPropertyName("platform")]
        public SystemInfoPlatform Platform { get; set; }
        
        [JsonPropertyName("serial_no")]
        public string SerialNumber { get; set; }
        
        [JsonPropertyName("device_id")]
        public string DeviceID { get; set; }
    }
}
