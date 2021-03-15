namespace SteamServerTool.SteamServerToolDll
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class SteamServerInfo
    {
        /// <summary>
        /// Name of the server in the context of this auto-updater, does not correspond to your actual server name
        /// 
        /// It is used to save the version of the file and distinguish if we have checked for the server version yet or not.
        /// </summary>
        [DataMember]
        public string ServerName { get; set; }

        /// <summary>
        /// Cmd to run the server
        /// For example:
        /// "C:\\valheim\\valheim_server.exe
        /// </summary>
        [DataMember]
        public string StartServerCmd { get; set; }

        /// <summary>
        /// Args to run the server including all flags, paired with the above command
        /// For example:
        /// "-nographics -batchmode -name \"My Server Name\" -port 2456 -world \"My World Name\" -password \"mypassword\" -public 0"
        /// </summary>
        [DataMember]
        public string StartServerArgs { get; set; }

        /// <summary>
        /// Optional parameter, only required if the run server command requires any environment variables be set.
        /// 
        /// For example, the valheim server requires Env variable "SteamAppId" which is set to the app id
        /// </summary>
        [DataMember]
        public Dictionary<string, string> EnvironmentArgs { get; set; }

        /// <summary>
        /// Where to install the server
        /// </summary>
        [DataMember]
        public string ServerInstallLocation { get; set; }

        /// <summary>
        /// This is used to check the 
        /// </summary>
        [DataMember]
        public int ServerInstallationId { get; set; }

        /// <summary>
        /// This is used to check the current version of the server. For example, Valheim game version is checked at the game page:
        /// 892970, however the server is installed with id 896660
        /// </summary>
        [DataMember]
        public int ServerUpdateId { get; set; }
    }
}
