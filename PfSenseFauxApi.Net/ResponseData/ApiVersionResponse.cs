using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class ApiVersionResponse
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
