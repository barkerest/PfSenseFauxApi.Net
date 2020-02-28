using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class RuleGetResponse
    {
        [JsonPropertyName("rules")]
        public RuleGetEntry[] Rules { get; set; }
    }
}
