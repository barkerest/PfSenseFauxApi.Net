using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class ConfigBackupResponse
    {
        [JsonPropertyName("backup_config_file")]
        public string BackupConfigFile { get; set; }
    }
}
