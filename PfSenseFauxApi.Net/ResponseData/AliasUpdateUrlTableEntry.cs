using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class AliasUpdateUrlTableEntry
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("status")]
        public string[] Status { get; set; }
    }
}
