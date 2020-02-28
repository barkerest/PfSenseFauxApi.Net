using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class GatewayStatusEntry
    {
        [JsonPropertyName("monitorip")]
        public string MonitorIP { get; set; }
        
        [JsonPropertyName("srcip")]
        public string SourceIP { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("delay")]
        public string Delay { get; set; }
        
        [JsonPropertyName("stddev")]
        public string StandardDeviation { get; set; }
        
        [JsonPropertyName("loss")]
        public string Loss { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
