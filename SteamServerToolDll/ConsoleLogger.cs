namespace SteamServerTool.SteamServerToolDll
{
    using System;
    using System.Globalization;

    public class ConsoleLogger : ILog
    {
        public bool ThrowOnError { get; set; }

        public void Error(string format, params object[] args)
        {
            string message = string.Format(CultureInfo.InvariantCulture, format, args);
            Console.WriteLine(message);
            if (ThrowOnError)
            {
                throw new Exception(message);
            }
        }

        public void Info(string format, params object[] args)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Warn(string format, params object[] args)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
        }
    }
}
