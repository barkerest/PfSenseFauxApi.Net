using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class UrlTableUpdateStatus
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        
        [JsonPropertyName("status")]
        public string[] Status { get; set; }
    }
}
