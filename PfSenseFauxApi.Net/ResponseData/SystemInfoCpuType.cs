using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class SystemInfoCpuType
    {
        [JsonPropertyName("cpu_model")]
        public string Model { get; set; }
        
        [JsonPropertyName("cpu_count")]
        public string Count { get; set; }
        
        [JsonPropertyName("logic_cpu_count")]
        public string LogicalCount { get; set; }
        
        [JsonPropertyName("cpu_freq")]
        public string Frequency { get; set; }
    }
}
