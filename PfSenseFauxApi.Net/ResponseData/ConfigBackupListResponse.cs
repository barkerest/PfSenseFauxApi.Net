using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class ConfigBackupListResponse
    {
        [JsonPropertyName("backup_files")]
        public ConfigBackupItem[] BackupFiles { get; set; }
    }
}
