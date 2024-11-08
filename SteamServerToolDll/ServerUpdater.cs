namespace SteamServerTool.SteamServerToolDll
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a game version as pulled from steam api
    /// 
    /// Example: {"game":{"gameName":"Valheim","gameVersion":"3","availableGameStats":{}}}
    /// </summary>
    [DataContract]
    internal class GameVersion
    {
        [DataMember]
        public string gameName;

        [DataMember]
        public string gameVersion;

        [DataMember]
        public Dictionary<object, object> availableGameStats;
    }

    internal class ServerUpdater
    {
        private readonly SteamServerToolConfig config;

        private readonly ILog logger;

        private readonly CancellationToken cancellationToken;

        private readonly string versionFileName;

        public ServerUpdater(SteamServerToolConfig config, CancellationToken token, ILog log)
        {
            this.config = config;
            this.logger = log;
            this.cancellationToken = token;
            string serverNameStripped = Regex.Replace(config.ServerInfo.ServerName, "[^a-zA-Z0-9]", "");
            this.versionFileName = string.Format(SteamServerToolConstants.VersionFile, serverNameStripped.ToLower());
        }

        private ServerUpdater()
        {
        }

        public async Task<bool> UpdatesRequired()
        {
            string currentVersion = GetCurrentVersion();

            // Haven't written our version file yet
            if (string.IsNullOrEmpty(currentVersion))
            {
                return true;
            }

            string onlineVersion = await GetVersion(config.ServerInfo.ServerUpdateId, config.SteamApiKey);

            if (string.IsNullOrEmpty(onlineVersion))
            {
                string errorMessage = $"Failed to get steam version of server from online source. Is your steam key and update ID correct?" +
                        $"\r\nPlease ensure the config is properly set at {SteamServerToolConstants.ConfigLocation}. If you need a steam API key " +
                        $"go to 'http://steamcommunity.com/dev/apikey'";

                this.logger.Error(errorMessage);
            }

            return currentVersion != onlineVersion;
        }

        public async Task<string> GetVersion(int appId, string key)
        {
            string versionJson = await GetVersionJson(appId, key);
            return await ExtractVersion(versionJson);

        }

        public async static Task<string> GetVersionJson(int appId, string key)
        {
            HttpClient client = new HttpClient();
            string uri = string.Format(SteamServerToolConstants.VersionQueryUri, key, appId);
            return await client.GetStringAsync(uri);
        }

        public async static Task<string> GetGameNameAsync(int appId, string key, ILog log)
        {
            string versionJson = await GetVersionJson(appId, key);
            string name = "";

            try
            {
                Dictionary<string, GameVersion> steamVersionInfo = ConfigUtil.GetConfigFromString<Dictionary<string, GameVersion>>(versionJson, ConfigFileFormat.Json);

                if (steamVersionInfo != null && steamVersionInfo.ContainsKey(SteamServerToolConstants.VersionGameKey))
                {
                    name = steamVersionInfo[SteamServerToolConstants.VersionGameKey].gameName;
                }
            }
            catch (Exception ex)
            {
                log.Info("Failed to get game name: '{0}'", ex.ToString());
            }

            return name;
        }

        private async Task<string> ExtractVersion(string verionInfoJson)
        {
            string version = string.Empty;

            try
            {
                Dictionary<string, GameVersion> steamVersionInfo = ConfigUtil.GetConfigFromString<Dictionary<string, GameVersion>>(verionInfoJson, ConfigFileFormat.Json);

                if (steamVersionInfo != null && steamVersionInfo.ContainsKey(SteamServerToolConstants.VersionGameKey))
                {
                    version = steamVersionInfo[SteamServerToolConstants.VersionGameKey].gameVersion;
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.ToString());
            }

            return await Task.FromResult(version);
        }

        private string GetCurrentVersion()
        {
            string version = string.Empty;
            
            try
            {
                Dictionary<string, GameVersion> versionInfo = ConfigUtil.GetConfig<Dictionary<string, GameVersion>>(this.versionFileName, ConfigFileFormat.Json, this.logger);
                if (versionInfo != null && versionInfo.ContainsKey(SteamServerToolConstants.VersionGameKey))
                {
                    version = versionInfo[SteamServerToolConstants.VersionGameKey].gameVersion;
                }
            }
            catch (FileNotFoundException ex)
            {
                this.logger.Info("Could not get current version: '{0}' ", ex.Message);
            }

            return version;
        }

        public async Task<bool> UpdateServer()
        {
            Process updateProcess;

            try
            {
                string cmdArgs = string.Format(SteamServerToolConstants.SteamCmdUpdateArgs,
                                                config.Username,
                                                config.Password,
                                                config.ServerInfo.ServerInstallLocation,
                                                config.ServerInfo.ServerInstallationId);

                ProcessStartInfo pInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = config.SteamCmdLocation,
                    Arguments = cmdArgs
                };

                updateProcess = Process.Start(pInfo);

                this.logger.Info("Starting update process. Will wait 60 minutes for update to finish");
                updateProcess.WaitForExit((int)TimeSpan.FromMinutes(60).TotalMilliseconds);

                if (!updateProcess.HasExited)
                {
                    throw new Exception("Failed to update server in timely manner.");
                }
                this.logger.Info($"Server updated successfully. Writing new config version to {System.IO.Directory.GetCurrentDirectory()}\\{this.versionFileName}");

                string versionJson = await GetVersionJson(config.ServerInfo.ServerUpdateId, config.SteamApiKey);

                ConfigUtil.WriteJsonConfig(this.versionFileName, versionJson);
            }
            catch (Exception ex)
            {
                this.logger.Info("Error while trying to start server! Exception: {0}", ex.Message);
            }

            return true;
        }
    }
}
