namespace SteamServerTool.SteamServerToolDll
{
    public interface ILog
    {
        bool ThrowOnError { get; set; }

        void Info(string format, params object[] args);

        void Warn(string format, params object[] args);

        void Error(string format, params object[] args);
    }
}
