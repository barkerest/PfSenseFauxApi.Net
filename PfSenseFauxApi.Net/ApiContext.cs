using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using PfSenseFauxApi.Net.Exceptions;
using PfSenseFauxApi.Net.ResponseData;

namespace PfSenseFauxApi.Net
{
    /// <summary>
    /// The API context definition.
    /// </summary>
    public class ApiContext
    {
        /// <summary>
        /// Path to the API.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The key used for the API.
        /// </summary>
        public AuthorizationKey AuthorizationKey { get; }

        /// <summary>
        /// Determines if the device certificate should be verified.
        /// </summary>
        public bool VerifySslCert { get; }

        /// <summary>
        /// Gets the version of the API.  Major, minor, and release come from the API.  Build is always zero.
        /// </summary>
        public Version ApiVersion { get; }

        
        private static readonly string VersionPattern = @"^(?<M>\d+)\.(?<N>\d+)\.f38(?:_(?<R>\d+))?$";
        
        /// <summary>
        /// Constructs a new API context.
        /// </summary>
        /// <param name="path">The path to the API (eg - "https://my.device/fauxapi/v1")</param>
        /// <param name="key">The authorization key.</param>
        /// <param name="verifySslCert">Should we verify the device's SSL certificate.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ApiException"></exception>
        public ApiContext(string path, AuthorizationKey key, bool verifySslCert)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrEmpty(Path)) throw new ArgumentException("Path cannot be blank.");
            AuthorizationKey = key ?? throw new ArgumentNullException(nameof(key));
            VerifySslCert = verifySslCert;

            try
            {
                var data  = StandardCall<ApiVersionResponse>("api_version").Result;
                var match = Regex.Match(data.Version, VersionPattern, RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    throw new ApiException($"The device does not appear to be running the F38 API fork ({data.Version}).");
                }

                var m = int.Parse(match.Groups["M"].Value);
                var n = int.Parse(match.Groups["N"].Value);
                var r = 0;
                if (match.Groups.ContainsKey("R"))
                {
                    int.TryParse(match.Groups["R"].Value, out r);
                }
                ApiVersion = new Version(m, n, 0, r);
            }
            catch (ApiMissingActionException)
            {
                ApiVersion = new Version(0, 0, 0, 0);
            }
        }

        #region Internals

        private static bool AcceptAnyCert(
            object          sender,
            X509Certificate certificate,
            X509Chain       chain,
            SslPolicyErrors sslPolicyErrors
        )
        {
            return true;
        }

        private HttpWebRequest CreateRequest(
            string                        endpoint,
            string                        method = "GET",
            IEnumerable<(string, string)> args   = null
        )
        {
            var url = Path + "?action=" + HttpUtility.UrlEncode(endpoint);

            if (args != null)
            {
                url += "&" + string.Join(
                           "&",
                           args.Where(x => !ReferenceEquals(null, x.Item2) && !string.IsNullOrEmpty(x.Item1))
                               .Select(x => HttpUtility.UrlEncode(x.Item1) + "=" + HttpUtility.UrlEncode(x.Item2))
                       );
                url = url.TrimEnd('&');
            }

            var ret = (HttpWebRequest) WebRequest.Create(url);

            if (!VerifySslCert)
            {
                ret.ServerCertificateValidationCallback = AcceptAnyCert;
            }

            ret.Method = method;
            ret.Headers.Add("fauxapi-auth", AuthorizationKey.GenerateToken());
            ret.Headers.Add("Accept", "application/json");

            return ret;
        }

        private async Task<string> GetResponseBody(HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream is object)
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        return await streamReader.ReadToEndAsync();
                    }
                }
            }

            return "";
        }

        private async Task<string> Execute(string endpoint, object data = null, IEnumerable<(string, string)> args = null, string method = "GET", HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var request = CreateRequest(endpoint, method, args);
            if (data != null)
            {
                request.Headers.Add("Content-Type", "application/json");
                using (var stream = await request.GetRequestStreamAsync())
                using (var writer = new StreamWriter(stream))
                {
                    var json = JsonSerializer.Serialize(data);
                    writer.Write(json);
                }
            }

            var response     = (HttpWebResponse) await request.GetResponseAsync();

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ApiMissingActionException(endpoint);
            }
            
            var responseBody = await GetResponseBody(response);

            if (response.StatusCode != expectedStatusCode)
            {
                throw new ApiHttpException(response.StatusCode, responseBody);
            }

            return responseBody;
        }

        private async Task<TResult> Execute<TResult>(string endpoint, object data = null, IEnumerable<(string, string)> args = null, string method = "GET", HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var responseBody = await Execute(endpoint, data, args, method, expectedStatusCode);

            if (string.IsNullOrWhiteSpace(responseBody)) return default;

            try
            {
                return JsonSerializer.Deserialize<TResult>(responseBody);
            }
            catch (JsonException e)
            {
                throw new ApiJsonException(responseBody, e);
            }
        }

        private async Task<TResult> StandardCall<TResult>(string endpoint, object data = null, IEnumerable<(string, string)> args = null, string method = "GET", HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var result = await Execute<Response<TResult>>(endpoint, data, args, method, expectedStatusCode);
            result.TestOk();
            return result.Data;
        }

        #endregion

        /// <summary>
        /// Causes the pfSense host to immediately update any urltable alias entries from their (remote) source URLs. Optionally update just one table by specifying the table name, else all tables are updated.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task<AliasUpdateUrlTablesResponse> AliasUpdateUrlTablesAsync(string tableName = null)
            => await StandardCall<AliasUpdateUrlTablesResponse>("alias_update_urltables", args: new[] {("table", tableName)});

        /// <summary>
        /// Causes the pfSense host to immediately update any urltable alias entries from their (remote) source URLs. Optionally update just one table by specifying the table name, else all tables are updated.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public AliasUpdateUrlTablesResponse AliasUpdateUrlTables(string tableName = null)
            => AliasUpdateUrlTablesAsync(tableName).Result;

        /// <summary>
        ///  Causes the system to take a configuration backup and add it to the regular set of pfSense system backups at /cf/conf/backup/
        /// </summary>
        /// <returns></returns>
        public async Task<ConfigBackupResponse> ConfigBackupAsync()
            => await StandardCall<ConfigBackupResponse>("config_backup");

        /// <summary>
        ///  Causes the system to take a configuration backup and add it to the regular set of pfSense system backups at /cf/conf/backup/
        /// </summary>
        /// <returns></returns>
        public ConfigBackupResponse ConfigBackup() => ConfigBackupAsync().Result;


        /// <summary>
        /// Returns a list of the currently available pfSense system configuration backups.
        /// </summary>
        /// <returns></returns>
        public async Task<ConfigBackupListResponse> ConfigBackupListAsync()
            => await StandardCall<ConfigBackupListResponse>("config_backup_list");

        /// <summary>
        /// Returns a list of the currently available pfSense system configuration backups.
        /// </summary>
        /// <returns></returns>
        public ConfigBackupListResponse ConfigBackupList()
            => ConfigBackupListAsync().Result;

        /// <summary>
        /// Returns the system configuration as a JSON formatted string. Additionally, using the optional config_file parameter it is possible to retrieve backup configurations by providing the full path to it under the /cf/conf/backup path.
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        /// <exception cref="ApiMessageException"></exception>
        /// <exception cref="ApiJsonException"></exception>
        public async Task<ConfigGetResponse> ConfigGetAsync(string configFile = null)
            => await StandardCall<ConfigGetResponse>("config_get", args: new[] {("config_file", config_file: configFile)});
        /*public async Task<JsonElement> ConfigGetAsync(string configFile = null)
        {
            var responseBody = await Execute("config_get", args: new []{("config_file", configFile)});

            if (string.IsNullOrWhiteSpace(responseBody)) return default;

            try
            {
                var root = JsonDocument.Parse(responseBody).RootElement;

                if (!root.TryGetProperty("message", out var msgElement))
                {
                    throw new ApiException("Missing message property.");
                }

                var msg = msgElement.GetString();
                if (!string.Equals("ok", msg, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ApiMessageException(msg);
                }

                if (!root.TryGetProperty("data", out var dataElement))
                {
                    throw new ApiException("Missing data property.");
                }

                return dataElement;
            }
            catch (JsonException e)
            {
                throw new ApiJsonException(responseBody, e);
            }
        }*/

        /// <summary>
        /// Returns the system configuration as a JSON formatted string. Additionally, using the optional config_file parameter it is possible to retrieve backup configurations by providing the full path to it under the /cf/conf/backup path.
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        /// <exception cref="ApiMessageException"></exception>
        /// <exception cref="ApiJsonException"></exception>
        public ConfigGetResponse ConfigGet(string configFile = null)
            => ConfigGetAsync(configFile).Result;

        /// <summary>
        /// Allows the API user to patch the system configuration with the existing system config
        /// </summary>
        /// <param name="data"></param>
        /// <param name="doBackup"></param>
        /// <param name="doReload"></param>
        /// <returns></returns>
        public async Task<ConfigUpdateResponse> ConfigPatchAsync(JsonElement data, bool doBackup = true, bool doReload = true)
            => await StandardCall<ConfigUpdateResponse>("config_patch", data: data, args: new[] {("do_backup", doBackup.ToString()), ("do_reload", doReload.ToString())}, method: "POST");

        /// <summary>
        /// Allows the API user to patch the system configuration with the existing system config
        /// </summary>
        /// <param name="data"></param>
        /// <param name="doBackup"></param>
        /// <param name="doReload"></param>
        /// <returns></returns>
        public ConfigUpdateResponse ConfigPatch(JsonElement data, bool doBackup = true, bool doReload = true)
            => ConfigPatchAsync(data, doBackup, doReload).Result;


        /// <summary>
        /// Causes the pfSense system to perform a reload action of the config.xml file, by default this happens when the config_set action occurs hence there is normally no need to explicitly call this after a config_set action.
        /// </summary>
        public async Task ConfigReloadAsync()
        {
            var result = await Execute<Response>("config_reload");
            result.TestOk();
        }

        /// <summary>
        /// Causes the pfSense system to perform a reload action of the config.xml file, by default this happens when the config_set action occurs hence there is normally no need to explicitly call this after a config_set action.
        /// </summary>
        public void ConfigReload()
            => ConfigReloadAsync().Wait();

        /// <summary>
        /// Restores the pfSense system to the named backup configuration.
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public async Task<ConfigRestoreResponse> ConfigRestoreAsync(string configFile)
            => await StandardCall<ConfigRestoreResponse>("config_restore", args: new[] {("config_file", configFile)});

        /// <summary>
        /// Restores the pfSense system to the named backup configuration.
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public ConfigRestoreResponse ConfigRestore(string configFile)
            => ConfigRestoreAsync(configFile).Result;

        /// <summary>
        /// Sets a full system configuration and (by default) takes a system config backup and (by default) causes the system config to be reloaded once successfully written and tested.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="doBackup"></param>
        /// <param name="doReload"></param>
        /// <returns></returns>
        public async Task<ConfigUpdateResponse> ConfigSetAsync(JsonElement data, bool doBackup = true, bool doReload = true)
            => await StandardCall<ConfigUpdateResponse>("config_set", data: data, args: new[] {("do_backup", doBackup.ToString()), ("do_reload", doReload.ToString())}, method: "POST");

        /// <summary>
        /// Sets a full system configuration and (by default) takes a system config backup and (by default) causes the system config to be reloaded once successfully written and tested.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="doBackup"></param>
        /// <param name="doReload"></param>
        /// <returns></returns>
        public ConfigUpdateResponse ConfigSet(JsonElement data, bool doBackup = true, bool doReload = true)
            => ConfigSetAsync(data, doBackup, doReload).Result;

        /// <summary>
        /// Returns gateway status data.
        /// </summary>
        /// <returns></returns>
        public async Task<GatewayStatusResponse> GatewayStatusAsync()
            => await StandardCall<GatewayStatusResponse>("gateway_status");

        /// <summary>
        /// Returns gateway status data.
        /// </summary>
        /// <returns></returns>
        public GatewayStatusResponse GatewayStatus()
            => GatewayStatusAsync().Result;

        /// <summary>
        /// Returns interface statistics data and information - the real interface name must be provided not an alias of the interface such as "WAN" or "LAN"
        /// </summary>
        /// <param name="interfaceName"></param>
        /// <returns></returns>
        public async Task<InterfaceStatsResponse> InterfaceStatsAsync(string interfaceName)
            => await StandardCall<InterfaceStatsResponse>("interface_stats", args: new[] {("interface", interfaceName)});

        /// <summary>
        /// Returns interface statistics data and information - the real interface name must be provided not an alias of the interface such as "WAN" or "LAN"
        /// </summary>
        /// <param name="interfaceName"></param>
        /// <returns></returns>
        public InterfaceStatsResponse InterfaceStats(string interfaceName)
            => InterfaceStatsAsync(interfaceName).Result;

        /// <summary>
        /// Returns the numbered list of loaded pf rules from a pfctl -sr -vv command on the pfSense host. An empty rule_number parameter causes all rules to be returned.
        /// </summary>
        /// <param name="ruleNumber"></param>
        /// <returns></returns>
        public async Task<RuleGetResponse> RuleGetAsync(int? ruleNumber = null)
            => await StandardCall<RuleGetResponse>("rule_get", args: new[] {("rule_number", ruleNumber?.ToString())});

        /// <summary>
        /// Returns the numbered list of loaded pf rules from a pfctl -sr -vv command on the pfSense host. An empty rule_number parameter causes all rules to be returned.
        /// </summary>
        /// <param name="ruleNumber"></param>
        /// <returns></returns>
        public RuleGetResponse RuleGet(int? ruleNumber = null)
            => RuleGetAsync(ruleNumber).Result;

        /// <summary>
        /// Just as it says, reboots the system.
        /// </summary>
        public async Task SystemRebootAsync()
        {
            var result = await Execute<Response>("system_reboot");
            result.TestOk();
        }

        /// <summary>
        /// Just as it says, reboots the system.
        /// </summary>
        public void SystemReboot()
            => SystemRebootAsync().Wait();

        /// <summary>
        /// Returns various useful system stats.
        /// </summary>
        /// <returns></returns>
        public async Task<SystemStatsResponse> SystemStatsAsync()
            => await StandardCall<SystemStatsResponse>("system_stats");

        /// <summary>
        /// Returns various useful system stats.
        /// </summary>
        /// <returns></returns>
        public SystemStatsResponse SystemStats()
            => SystemStatsAsync().Result;

        /// <summary>
        /// Returns various useful system info.
        /// </summary>
        /// <returns></returns>
        public async Task<SystemInfoResponse> SystemInfoAsync()
            => await StandardCall<SystemInfoResponse>("system_info");

        /// <summary>
        /// Returns various useful system info.
        /// </summary>
        /// <returns></returns>
        public SystemInfoResponse SystemInfo()
            => SystemInfoAsync().Result;


        // TODO: function_call helpers
        // TODO: send_event helpers
    }
}
