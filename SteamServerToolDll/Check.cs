namespace SteamServerTool.SteamServerToolDll
{
    using System;
    using System.IO;
    using System.Globalization;

    public static class Check
    {
        public static class FileArg
        {
            public static void Exists(string filename, string argumentName)
            {
                if (!File.Exists(filename))
                {
                    throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0} must be a location that exists. {1} does not exist", argumentName, filename));
                }
            }
        }

        public static class Argument
        {
            public static void NotNull(object argument, string argumentName)
            {
                if (argument == null)
                {
                    throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0} cannot be null", argumentName));
                }
            }

            public static void IsPositive(int argument, string argumentName)
            {
                if (argument <= 0)
                {
                    throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0} must be > 0", argumentName));
                }
            }

            public static void NotNullOrEmpty(string argument, string argumentName)
            {
                if (string.IsNullOrEmpty(argument))
                {
                    throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0} cannot be null or empty", argumentName));
                }
            }

            public static void NotNullOrEmpty(string argument, string argumentName, string extraInfo)
            {
                if (string.IsNullOrEmpty(argument))
                {
                    throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "{0} cannot be null or empty.\r\n{1}", argumentName, extraInfo));
                }
            }
        }
    }
}
