using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class SystemStatsResponse
    {
        [JsonPropertyName("stats")]
        public SystemStatsEntry Stats { get; set; }
    }
}
