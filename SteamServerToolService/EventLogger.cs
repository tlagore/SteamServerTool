
namespace SteamServerTool.SteamServerToolDll
{
    using System.Diagnostics;
    using System.Globalization;

    public class EventLogger : ILog
    {
        private readonly EventLog eventLog;

        public EventLogger(EventLog log)
        {
            this.eventLog = log;
        }

        public bool ThrowOnError { get; set; }

        public void Error(string format, params object[] args)
        {
            string message = string.Format(CultureInfo.InvariantCulture, format, args);
            this.eventLog.WriteEntry(message, EventLogEntryType.Error);

            if (ThrowOnError)
            {
                throw new System.Exception(message);
            }
        }

        public void Info(string format, params object[] args)
        {
            this.eventLog.WriteEntry(string.Format(CultureInfo.InvariantCulture, format, args), EventLogEntryType.Information);
        }

        public void Warn(string format, params object[] args)
        {
            this.eventLog.WriteEntry(string.Format(CultureInfo.InvariantCulture, format, args), EventLogEntryType.Warning);
        }
    }
}
