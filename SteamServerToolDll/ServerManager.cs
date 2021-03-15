namespace SteamServerTool.SteamServerToolDll
{
    using System;
    using System.Globalization;
    using System.Threading;
    using SystemTimer = System.Timers.Timer;

    public sealed class ServerManager
    {
        private readonly ServerUpdater serverUpdater;

        private readonly ServerProcessManager serverProcessManager;

        private readonly ILog logger;

        private readonly CancellationToken cancellationToken;

        private readonly SystemTimer updateTimer;

        public ServerManager(SteamServerToolConfig config, CancellationToken token, ILog log)
        {
            Check.Argument.NotNull(config, nameof(config));

            this.logger = log;

            this.VerifyConfig(config);

            this.cancellationToken = token;
            this.serverUpdater = new ServerUpdater(config, token, log);
            this.serverProcessManager = new ServerProcessManager(config.ServerInfo, token, log);

            this.updateTimer = new SystemTimer()
            {
                AutoReset = false,
                Interval = TimeSpan.FromSeconds(config.ServerUpdateTimeSeconds).TotalMilliseconds,
            };

            this.updateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private async void VerifyConfig(SteamServerToolConfig config)
        {
            bool changed = false;

            int minUpdateInterval = 60;

            Check.Argument.NotNullOrEmpty(config.Username,
                                    nameof(config.Username));
            Check.Argument.NotNullOrEmpty(config.SteamApiKey,
                                    nameof(config.SteamApiKey),
                                    "Steam API Key can be obtained at 'http://steamcommunity.com/dev/apikey'");
            Check.Argument.NotNullOrEmpty(config.SteamCmdLocation, 
                                    nameof(config.SteamCmdLocation),
                                    "Steamcmd can be obtained at 'http://media.steampowered.com/installer/steamcmd.zip'");
            Check.FileArg.Exists(config.SteamCmdLocation, nameof(config.SteamCmdLocation));

            Check.Argument.IsPositive(config.ServerInfo.ServerInstallationId, nameof(config.ServerInfo.ServerInstallationId));
            Check.Argument.IsPositive(config.ServerInfo.ServerUpdateId, nameof(config.ServerInfo.ServerUpdateId));

            string gameName = await ServerUpdater.GetGameNameAsync(config.ServerInfo.ServerUpdateId, config.SteamApiKey, this.logger);

            if (config.ServerUpdateTimeSeconds < minUpdateInterval)
            {
                this.logger.Info("{0} was set to lower than {1} seconds. Set the interval to {1} seconds.", nameof(config.ServerUpdateTimeSeconds), minUpdateInterval);
                config.ServerUpdateTimeSeconds = minUpdateInterval;
                changed = true;
            }

            if (!string.Equals(config.Username, SteamServerToolConstants.AnonymousUser))
            {
                if (string.IsNullOrEmpty(config.Password))
                {
                    throw new Exception(string.Format(CultureInfo.InvariantCulture, "Password cannot be empty if username is not '{0}'", SteamServerToolConstants.AnonymousUser));
                }
            }

            Check.Argument.NotNull(config.ServerInfo, nameof(config.ServerInfo));
            
            Check.Argument.NotNullOrEmpty(config.ServerInfo.StartServerCmd, nameof(config.ServerInfo.StartServerCmd));
            Check.Argument.NotNullOrEmpty(config.ServerInfo.ServerInstallLocation, nameof(config.ServerInfo.StartServerCmd));

            if (string.IsNullOrEmpty(config.ServerInfo.ServerName))
            {
                Guid guid = new Guid();
                string nameFormat = "{0}{1}";
                string name = string.Format(nameFormat, gameName != null ? $"{gameName}_" : "", guid);
                this.logger.Info("{0} was empty or null, assigning random name: {1}.", 
                    nameof(config.ServerInfo.ServerName),
                    name);

                config.ServerInfo.ServerName = name;
                changed = true;
            }

            if (changed)
            {
                this.logger.Info("Config validation changed the configuration, writing new configurations to file.");
                ConfigUtil.WriteJsonConfig(SteamServerToolConstants.ConfigLocation, config);
            }
        }

        private async void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.cancellationToken.IsCancellationRequested)
            {
                this.logger.Info("Cancellation is requested, killing server.");

                if (!this.serverProcessManager.StopServer())
                {
                    this.logger.Error("Failed to stop the server, not going to update.");
                }

                this.cancellationToken.ThrowIfCancellationRequested();
            }

            this.updateTimer.Stop();

            this.logger.Info("Checking for updates..");

            if (await this.serverUpdater.UpdatesRequired())
            {
                if (!this.serverProcessManager.StopServer())
                {
                    this.logger.Error("Failed to stop the server, not going to update.");
                }
                else
                {
                    if (!await this.serverUpdater.UpdateServer())
                    {
                        this.logger.Error("Unable to update server!");
                    }
                }

                if (!this.serverProcessManager.StartServer(false))
                {
                    this.logger.Error("Failed to start the server.");
                }
            }

            if (!this.serverProcessManager.ServerRunning)
            {
                if (!this.serverProcessManager.StartServer(false))
                {
                    this.logger.Error("Failed to start the server.");
                }
            }

            this.updateTimer.Start();
        }

        public async void Start()
        {
            if (await this.serverUpdater.UpdatesRequired())
            {
                if (!await this.serverUpdater.UpdateServer())
                {
                    this.logger.Error("Failed to update server!");
                }
            }

            if (!this.serverProcessManager.StartServer(false))
            {
                this.logger.Error("Failed to start the server!");
            }

            this.updateTimer.Start();
        }

        public void Stop()
        {
            this.updateTimer?.Stop();

            if (!this.serverProcessManager.StopServer())
            {
                this.logger.Error("Failed to stop the server.");
            }
        }
    }
}
