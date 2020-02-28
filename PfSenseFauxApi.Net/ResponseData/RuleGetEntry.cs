using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class RuleGetEntry
    {
        [JsonPropertyName("number")]
        public int Number { get; set; }
        
        [JsonPropertyName("rule")]
        public string Rule { get; set; }
        
        [JsonPropertyName("evaluations")]
        public string Evaluations { get; set; }
        
        [JsonPropertyName("packets")]
        public string Packets { get; set; }
        
        [JsonPropertyName("bytes")]
        public string Bytes { get; set; }
        
        [JsonPropertyName("states")]
        public string States { get; set; }
        
        [JsonPropertyName("inserted")]
        public string Inserted { get; set; }
        
        [JsonPropertyName("statecreations")]
        public string StateCreations { get; set; }
    }
}
