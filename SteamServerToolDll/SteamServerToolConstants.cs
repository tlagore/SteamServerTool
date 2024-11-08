namespace SteamServerTool.SteamServerToolDll
{ 
    public static class SteamServerToolConstants
    {
        public const string AnonymousUser = "anonymous";
        public const string ConfigLocation = "C:\\tools\\SteamServerTool\\sst_config.json";
        public const string VersionFile = "{0}_version.json";
        public const string VersionGameKey = "game";
        public const string Windows64ApplicationName = "FactoryServer-Win64-Shipping-Cmd";
        public const string VersionQueryUri = "http://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/?key={0}&appid={1}&format=json";
        /// <summary>
        /// {0} Username, {1} password (none if anonymous), {2} Installation directory, {3} Application ID
        /// </summary>
        public const string SteamCmdUpdateArgs = "+login {0} {1} +force_install_dir {2} +app_update {3} validate +quit";
    }
}
