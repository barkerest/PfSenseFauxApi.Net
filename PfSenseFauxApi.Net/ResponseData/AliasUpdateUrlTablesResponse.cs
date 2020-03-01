using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class AliasUpdateUrlTablesResponse
    {
        [JsonPropertyName("updates")]
        public AliasUpdateUrlTableEntry[] Updates { get; set; }
    }
    
    
}
