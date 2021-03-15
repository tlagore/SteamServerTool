namespace SteamServerTool.Service
{
    using SteamServerToolDll;
    using System;
    using System.Threading;

    class Program
    {
        private static ServerManager manager;
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Close);
            SteamServerToolConfig config = ConfigUtil.GetConfig<SteamServerToolConfig>(SteamServerToolConstants.ConfigLocation, ConfigFileFormat.Json);
            manager = new ServerManager(config, tokenSource.Token);
            IOUtil.Log("Starting server");
            manager.Start();

            while (true)
            {
                Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                IOUtil.Log("SteamServerTool Working...");
            }
        }

        public static void Close(object caller, ConsoleCancelEventArgs args)
        {
            IOUtil.Log("Shutting down...");
            manager.Stop();
            tokenSource.Cancel();
        }
    }
}
