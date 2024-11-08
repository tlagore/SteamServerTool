namespace SteamServerTool.SteamServerToolDll
{
    using Newtonsoft.Json;
    using System;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// File formats this ConfigReader Supports
    /// </summary>
    public enum ConfigFileFormat
    {
        Json
    }

    public static class ConfigUtil
    {
        public static T GetConfig<T>(string location, ConfigFileFormat format, ILog logger)
        {
            switch (format)
            {
                case ConfigFileFormat.Json:
                    string fileContent = ReadFile(location);
                    logger.Info("Attempting to parse file contents:");
                    logger.Info(fileContent.Replace("{","{{").Replace("}", "}}"));
                    return GetConfigFromJson<T>(fileContent);
                default:
                    throw new ArgumentException(Format("Unsupported file format '{0}'", format));
            }
        }

        public static T GetConfigFromString<T>(string str, ConfigFileFormat format)
        {
            switch (format)
            {
                case ConfigFileFormat.Json:
                    return GetConfigFromJson<T>(str);
                default:
                    throw new ArgumentException(Format("Unsupported file format '{0}'", format));
            }
        }

        public static string ReadFile(string location)
        {
            if (!File.Exists(location))
            {
                throw new FileNotFoundException(Format("{0}: '{1}' does not exist.", nameof(location), location));
            }

            return File.ReadAllText(location);
        }

        public static void WriteJsonConfig(string fileName, object config)
        {
            string configString = JsonConvert.SerializeObject(config);
            File.WriteAllText(fileName, configString);
        }

        public static void WriteJsonConfig(string fileName, string jsonConfig)
        {
            File.WriteAllText(fileName, jsonConfig);
        }

        private static T GetConfigFromJson<T>(string content)
        {
            return (T)JsonConvert.DeserializeObject<T>(content);
        }

        private static string Format(string message, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, message, args);
        }
    }
}
