using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class GatewayStatusResponse
    {
        [JsonPropertyName("gateway_status")]
        public Dictionary<string, GatewayStatusEntry> GatewayStatus { get; set; }
    }
}
