using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class SystemInfoResponse
    {
        [JsonPropertyName("info")]
        public SystemInfoEntry Info { get; set; }
    }
}
