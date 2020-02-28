using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class ConfigUpdateResponse
    {
        [JsonPropertyName("do_backup")]
        public bool DoBackup { get; set; }
        
        [JsonPropertyName("do_reload")]
        public bool DoReload { get; set; }
        
        [JsonPropertyName("previous_config_file")]
        public string PreviousConfigFile { get; set; }
    }
}
