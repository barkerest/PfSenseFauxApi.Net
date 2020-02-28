using System.Text.Json.Serialization;

namespace PfSenseFauxApi.Net.ResponseData
{
    public class SystemInfoEntry
    {
        
        [JsonPropertyName("sys")]
        public SystemInfoSysEntry System { get; set; }
        
        [JsonPropertyName("pfsense_version")]
        public SystemInfoPfSenseVersion PfSenseVersion { get; set; }
        
        [JsonPropertyName("pfsense_remote_version")]
        public SystemInfoPfSenseRemoteVersion PfSenseRemoteVersion { get; set; }
        
        [JsonPropertyName("os_version")]
        public string OsVersion { get; set; }
        
        [JsonPropertyName("cpu_type")]
        public SystemInfoCpuType CpuType { get; set; }
        
        [JsonPropertyName("kernel_pti_status")]
        public string KernelPtiStatus { get; set; }
        
        [JsonPropertyName("mds_mitigation")]
        public string MdsMitigation { get; set; }
        
        [JsonPropertyName("bios")]
        public SystemInfoBios Bios { get; set; }
    }
}
