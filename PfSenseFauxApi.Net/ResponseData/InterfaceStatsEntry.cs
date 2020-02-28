using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class InterfaceStatsEntry
    {
        [JsonPropertyName("inpkts")]
        public long InPackets { get; set; }
        
        [JsonPropertyName("inbytes")]
        public long InBytes { get; set; }
        
        [JsonPropertyName("outpkts")]
        public long OutPackets { get; set; }
        
        [JsonPropertyName("outbytes")]
        public long OutBytes { get; set; }
        
        [JsonPropertyName("inerrs")]
        public long InErrors { get; set; }
        
        [JsonPropertyName("outerrs")]
        public long OutErrors { get; set; }
        
        [JsonPropertyName("collisions")]
        public long Collisions { get; set; }
        
        [JsonPropertyName("inmcasts")]
        public long InMultiCasts { get; set; }
        
        [JsonPropertyName("outmcasts")]
        public long OutMultiCasts { get; set; }
        
        [JsonPropertyName("unsupproto")]
        public long UnsupportedProtocol { get; set; }
        
        [JsonPropertyName("mtu")]
        public int Mtu { get; set; }
    }
}
