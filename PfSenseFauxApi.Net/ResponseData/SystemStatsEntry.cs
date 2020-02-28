using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class SystemStatsEntry
    {
        [JsonPropertyName("cpu")]
        public string Cpu { get; set; }
        
        [JsonPropertyName("mem")]
        public string Memory { get; set; }
        
        [JsonPropertyName("uptime")]
        public string Uptime { get; set; }
        
        [JsonPropertyName("pfstate")]
        public string PfStates { get; set; }
        
        [JsonPropertyName("pfstatepercent")]
        public string PfStatePercent { get; set; }
        
        [JsonPropertyName("temp")]
        public string Temperature { get; set; }
        
        [JsonPropertyName("datetime")]
        public string DateTime { get; set; }
        
        [JsonPropertyName("cpufreq")]
        public string CpuFrequency { get; set; }
        
        [JsonPropertyName("load_average")]
        public string[] LoadAverage { get; set; }
        
        [JsonPropertyName("mbuf")]
        public string MemBufs { get; set; }
        
        [JsonPropertyName("mbufpercent")]
        public string MemBufPercent { get; set; }
    }
}
