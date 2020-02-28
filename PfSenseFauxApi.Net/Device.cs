﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using PfSenseFauxApi.Net.Exceptions;
using PfSenseFauxApi.Net.ResponseData;

namespace PfSenseFauxApi.Net
{
    public class Device
    {
        #region Key/Secret

        // Alphanumeric, 12-40 chars, start with PFFA, not be PFFAexample01 or PFFAexample02.
        private static readonly Regex ValidKey = new Regex(@"^PFFA(?!example0[12])[A-Za-z0-9]{8,36}$");

        // Alphanumeric, 40-128 chars.
        private static readonly Regex ValidSecret = new Regex(@"^[A-Za-z0-9]{40,128}$");

        private static readonly char[] AlphaNum = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        private static readonly RandomNumberGenerator RNG = RandomNumberGenerator.Create();
        private static readonly SHA256                SHA = SHA256.Create();

        private static string GenerateNonce()
        {
            var data = new byte[4];
            lock (RNG)
            {
                RNG.GetBytes(data);
            }

            var l = BitConverter.ToUInt32(data);
            return l.ToString("x8");
        }

        /// <summary>
        /// Generates a key/secret pair.
        /// </summary>
        /// <returns></returns>
        public static (string, string) GenerateKeyPair(int keyLen = 24, int secretLen = 60)
        {
            if (keyLen < 12 ||
                keyLen > 40) throw new ArgumentException("Key length must be between 12 and 40 characters.");
            if (secretLen < 40 ||
                secretLen > 128) throw new ArgumentException("Secret length must be between 40 and 128 characters.");

            keyLen -= 4;
            var data = new byte[keyLen + secretLen];

            while (true)
            {
                lock (RNG)
                {
                    RNG.GetBytes(data);
                }

                var sb = new StringBuilder("PFFA");
                for (var i = 0; i < keyLen; i++)
                {
                    sb.Append(AlphaNum[data[i] % AlphaNum.Length]);
                }

                var key = sb.ToString();

                sb.Clear();
                for (var i = 0; i < secretLen; i++)
                {
                    var j = i + keyLen;
                    sb.Append(AlphaNum[data[i] % AlphaNum.Length]);
                }

                var secret = sb.ToString();

                // theoretically possible, but highly unlikely.
                if (!ValidKey.IsMatch(key) ||
                    !ValidSecret.IsMatch(secret)) continue;

                return (key, secret);
            }
        }

        #endregion

        /// <summary>
        /// Path to the fauxapi.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The key used for the API.
        /// </summary>
        public string ApiKey { get; }

        /// <summary>
        /// The secret used to authenticate the API.
        /// </summary>
        public string ApiSecret { get; }

        /// <summary>
        /// Determines if the device certificate should be verified.
        /// </summary>
        public bool VerifySslCert { get; }

        public Device(string path, string key, string secret, bool verifySslCert)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrEmpty(Path)) throw new ArgumentException("Path cannot be blank.");
            ApiKey = key ?? throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(ApiKey)) throw new ArgumentException("ApiKey cannot be blank.");
            ApiSecret = secret ?? throw new ArgumentException(nameof(secret));
            if (string.IsNullOrEmpty(ApiSecret)) throw new ArgumentException("ApiSecret cannot be blank.");

            if (!ValidKey.IsMatch(key)) throw new ArgumentException("ApiKey is invalid.");
            if (!ValidSecret.IsMatch(secret)) throw new ArgumentException("ApiSecret is invalid.");

            VerifySslCert = verifySslCert;
        }

        #region Internals

        private string GenerateAuth()
        {
            var dt         = DateTime.Now.ToUniversalTime().ToString("yyyyMMdd'Z'HHMMss");
            var nonce      = GenerateNonce();
            var data       = Encoding.ASCII.GetBytes($"{ApiSecret}{dt}{nonce}");
            var hashBytes  = SHA.ComputeHash(data);
            var hashString = string.Join("", hashBytes.Select(x => x.ToString("x2")));
            return $"{ApiKey}:{dt}:{nonce}:{hashString}";
        }

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
                               .Select(x => HttpUtility.UrlEncode(x.Item1) + "=" + HttpUtility.UrlEncode(x.Item2)));
                url =  url.TrimEnd('&');
            }

            var ret = (HttpWebRequest) WebRequest.Create(url);

            if (!VerifySslCert)
            {
                ret.ServerCertificateValidationCallback = AcceptAnyCert;
            }

            ret.Method = method;
            ret.Headers.Add("fauxapi-auth", GenerateAuth());
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
        public async Task<JsonElement> ConfigGetAsync(string configFile = null)
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
        }

        /// <summary>
        /// Returns the system configuration as a JSON formatted string. Additionally, using the optional config_file parameter it is possible to retrieve backup configurations by providing the full path to it under the /cf/conf/backup path.
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        /// <exception cref="ApiMessageException"></exception>
        /// <exception cref="ApiJsonException"></exception>
        public JsonElement ConfigGet(string configFile = null) 
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