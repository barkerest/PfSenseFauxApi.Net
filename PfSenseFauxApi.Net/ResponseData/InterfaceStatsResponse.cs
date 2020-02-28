using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class InterfaceStatsResponse
    {
        [JsonPropertyName("stats")]
        public InterfaceStatsEntry Stats { get; set; }
    }
}
