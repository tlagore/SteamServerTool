
namespace SteamServerToolService
{
    using SteamServerTool.SteamServerToolDll;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.ServiceProcess;
    using System.Threading;

    public partial class SteamServerToolService : ServiceBase
    {
        private ServerManager manager;

        private CancellationTokenSource tokenSource;

        private ILog logger;

        public SteamServerToolService()
        {
            this.CanStop = true;
            this.CanShutdown = true;
            this.CanPauseAndContinue = false;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            EventLog log = new EventLog();

            ((ISupportInitialize)(this.EventLog)).BeginInit();
            this.EventLog.Source = "SteamServiceTool";
            this.EventLog.Log = "Application";
            if (!EventLog.SourceExists(this.EventLog.Source))
            {
                EventLog.CreateEventSource(this.EventLog.Source, this.EventLog.Log);
            }
            ((ISupportInitialize)(this.EventLog)).EndInit();

            log.Source = this.EventLog.Source;
            log.Log = this.EventLog.Log;
            tokenSource = new CancellationTokenSource();

            this.logger = new EventLogger(log);
            this.logger.Info($"Attempting to open config at {SteamServerToolConstants.ConfigLocation}");
            SteamServerToolConfig config = ConfigUtil.GetConfig<SteamServerToolConfig>(SteamServerToolConstants.ConfigLocation, ConfigFileFormat.Json, this.logger);
            this.manager = new ServerManager(config, tokenSource.Token, this.logger);
            this.logger.Info("Starting server");
            this.manager.Start();

            while (!tokenSource.Token.IsCancellationRequested)
            {
                Thread.Sleep((int)TimeSpan.FromSeconds(20).TotalMilliseconds);
                this.logger.Info("SteamServerTool Working...");
            }
        }

        protected override void OnShutdown()
        {
            this.logger.Info("Shutting down...");
            this.manager.Stop();
            this.tokenSource.Cancel();
        }

        protected override void OnStop()
        {
            this.logger.Info("Stopping...");
            this.OnShutdown();
        }
    }
}
