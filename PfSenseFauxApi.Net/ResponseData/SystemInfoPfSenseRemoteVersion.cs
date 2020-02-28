using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class SystemInfoPfSenseRemoteVersion
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        
        [JsonPropertyName("installed_version")]
        public string InstalledVersion { get; set; }
        
        [JsonPropertyName("pkg_version_compare")]
        public string PackageVersionCompare { get; set; }
    }
}
