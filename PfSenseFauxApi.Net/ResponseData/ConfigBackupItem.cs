using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class ConfigBackupItem
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }
        
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        [JsonPropertyName("version")]
        public string Version { get; set; }
        
        [JsonPropertyName("filesize")]
        public long FileSize { get; set; }
    }
}
