namespace SteamServerTool.SteamServerToolDll
{
    using System;
    using System.Globalization;

    public static class IOUtil
    {
        public static void FailIfFlagged(string message, bool failOnException)
        {
            if (failOnException)
            {
                throw new Exception($"FATAL:: {nameof(failOnException)} set to true. Exitting with message: '{message}'");
            }
            else
            {
                IOUtil.Log($"{message}");
            }
        }

        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void Log(string message, params object[] args)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, message, args));
        }
    }
}
