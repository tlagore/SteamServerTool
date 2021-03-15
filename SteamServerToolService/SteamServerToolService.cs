
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
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.ServiceName = "SteamServerTool";
            EventLog log = new EventLog();
            this.EventLog.Source = this.ServiceName;
            this.EventLog.Log = "Applicaiton";

            ((ISupportInitialize)(this.EventLog)).BeginInit();
            if (!EventLog.SourceExists(this.EventLog.Source))
            {
                EventLog.CreateEventSource(this.EventLog.Source, this.EventLog.Log);
            }
            ((ISupportInitialize)(this.EventLog)).EndInit();

            this.logger = new EventLogger(log);

            SteamServerToolConfig config = ConfigUtil.GetConfig<SteamServerToolConfig>(SteamServerToolConstants.ConfigLocation, ConfigFileFormat.Json);
            this.manager = new ServerManager(config, tokenSource.Token, this.logger);
            this.logger.Info("Starting server");
            this.manager.Start();
            this.CanStop = true;
            this.CanShutdown = true;
            this.CanPauseAndContinue = false;

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
