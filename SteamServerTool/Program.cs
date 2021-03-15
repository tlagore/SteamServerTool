namespace SteamServerTool.Service
{
    using SteamServerToolDll;
    using System;
    using System.Threading;

    class Program
    {
        private static ServerManager manager;
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();
        private static ILog logger = new ConsoleLogger();

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Close);
            SteamServerToolConfig config = ConfigUtil.GetConfig<SteamServerToolConfig>(SteamServerToolConstants.ConfigLocation, ConfigFileFormat.Json);

            logger.ThrowOnError = config.FailOnException;

            manager = new ServerManager(config, tokenSource.Token, logger);
            logger.Info("Starting server");
            manager.Start();

            while (true)
            {
                Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                logger.Info("SteamServerTool Working...");
            }
        }

        public static void Close(object caller, ConsoleCancelEventArgs args)
        {
            logger.Info("Shutting down...");
            manager.Stop();
            tokenSource.Cancel();
        }
    }
}
