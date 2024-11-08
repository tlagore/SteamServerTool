namespace SteamServerTool.SteamServerToolDll
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using SystemTimer = System.Timers.Timer;

    internal class ServerProcessManager
    {
        private readonly SteamServerInfo serverInfo;

        private readonly CancellationToken cancellationToken;

        private readonly ILog logger;

        private readonly object processLock = new object();

        private bool shouldBeRunning;

        private bool forceKill = true;

        private readonly double timeToWaitForExitMs = TimeSpan.FromMinutes(2).TotalMilliseconds;

        private readonly SystemTimer healthCheckTimer;

        private Process process;

        internal const int CTRL_C_EVENT = 0;


        [DllImport("kernel32.dll")]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        // Delegate type to be used as the Handler Routine for SCCH
        delegate Boolean ConsoleCtrlDelegate(uint CtrlType);

        public ServerProcessManager(SteamServerInfo serverInfo, CancellationToken token, ILog log)
        {
            this.serverInfo = serverInfo;
            this.cancellationToken = token;
            this.logger = log;

            this.healthCheckTimer = new SystemTimer()
            {
                AutoReset = false,
                Interval = TimeSpan.FromMinutes(1).TotalMinutes
            };

            this.healthCheckTimer.Elapsed += HealthCheckTimer_Elapsed;
            this.shouldBeRunning = false;
        }

        private void HealthCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.healthCheckTimer.Stop();

            this.cancellationToken.ThrowIfCancellationRequested();

            if (!this.shouldBeRunning)
            {
                return;
            }

            if (this.process == null || this.process.HasExited)
            {
                this.logger.Info("Process is null but {0}=true. Attempting to restart the server.", nameof(ServerRunning));
                if (!StartServer(true))
                {
                    this.logger.Info("Failed to start server!");
                }
            }

            this.healthCheckTimer.Start();
        }

        private ServerProcessManager()
        {
        }

        public bool ServerRunning { get; private set; }

        public bool StartServer(bool isHealthCheckRequest)
        {
            this.CheckIfCancelled();

            lock (processLock)
            {
                if (isHealthCheckRequest && !this.shouldBeRunning)
                {
                    return false;
                }

                if (ServerRunning && this.process != null && !this.process.HasExited)
                {
                    return true;
                }

                try
                {
                    if (this.process != null && !this.process.HasExited)
                    {
                        this.logger.Info("Server was still running when StartServer called. Attempting to stop server gracefully.");

                        if (!this.StopServer())
                        {
                            throw new Exception("Unable to gracefully kill server in StartServer(), cancelling.");
                        }
                    }

                    if (this.serverInfo.EnvironmentArgs != null && this.serverInfo.EnvironmentArgs.Count > 0)
                    {
                        foreach (var envValuePair in this.serverInfo.EnvironmentArgs)
                        {
                            this.logger.Info("Setting env variable '{0}'='{1}'", envValuePair.Key, envValuePair.Value);
                            Environment.SetEnvironmentVariable(envValuePair.Key, envValuePair.Value);
                        }
                    }

                    ProcessStartInfo pInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal,
                        FileName = this.serverInfo.StartServerCmd,
                        Arguments = this.serverInfo.StartServerArgs
                    };

                    this.process = Process.Start(pInfo);

                    ServerRunning = true;

                    healthCheckTimer.Start();

                    if (!isHealthCheckRequest)
                    {
                        this.shouldBeRunning = true;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Info("Error while trying to start server! Exception: {0}", ex.Message);
                }
            }

            return ServerRunning;
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        /// <returns>true if server has exitted</returns>
        public bool StopServer()
        {
            lock (this.processLock)
            {
                this.shouldBeRunning = false;

                if (!ServerRunning)
                {
                    return true;
                }

                try
                {
                    if (this.process == null)
                    {
                        throw new Exception("Server is running but process is null.");
                    }

                    if (this.process.HasExited)
                    {
                        return true;
                    }

                    // If this somehow ran as a console application we need to first free the console so that we can attach it to the running server
                    if (!FreeConsole())
                    {
                        throw new Exception($"Error when freeing console. System error code: {GetLastError()}. See https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes for specific error codes");
                    }

                    if (AttachConsole((uint)process.Id))
                    {
                        SetConsoleCtrlHandler(null, false);
                        try
                        {
                            if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                            {
                                this.logger.Info("Failed to end server gracefully!");

                                if (this.forceKill)
                                {
                                    process.Kill();
                                }
                            }

                            process.WaitForExit((int)timeToWaitForExitMs);

                            if (!process.HasExited)
                            {
                                throw new Exception("Process didn't exit in a proper amount of time.");
                            }
                            else
                            {
                                this.logger.Info("Server exitted gracefully.");
                            }

                            ServerRunning = false;
                        }
                        catch (Exception ex)
                        {
                            this.logger.Info($"Exception!! {ex.Message}");

                            try
                            {
                                if (this.process != null && !this.process.HasExited)
                                {
                                    this.logger.Info("Server had not exited, killing application.");
                                    this.process.Kill();
                                    this.ServerRunning = false;
                                }
                            }
                            catch (Exception innerEx)
                            {
                                Console.Write($"FATAL:: Failed to forcefully kill the server with exception {innerEx.Message}. Rethrowing.");
                                throw innerEx;
                            }
                        }
                        finally
                        {
                            FreeConsole();
                        }
                    }
                    else
                    {
                        throw new Exception($"Failed to AttachConsole with error code {GetLastError()}");
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Info($"Error while trying to start server! Exception: {ex.Message}");
                    ServerRunning = false;
                }
            }

            return !ServerRunning;
        }

        /// <summary>
        /// Try and Stop the server gracefully if cancelled
        /// </summary>
        private void CheckIfCancelled()
        {
            if (this.cancellationToken.IsCancellationRequested)
            {
                this.StopServer();
            }

            this.cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
