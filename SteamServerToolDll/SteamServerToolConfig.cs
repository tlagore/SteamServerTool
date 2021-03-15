namespace SteamServerTool.SteamServerToolDll
{
    using System.Runtime.Serialization;

    [DataContract]
    public class SteamServerToolConfig
    {
        /// <summary>
        /// Required to update the server obtain it here:
        /// http://media.steampowered.com/installer/steamcmd.zip
        /// </summary>
        [DataMember]
        public string SteamCmdLocation { get; set; }

        /// <summary>
        /// Api key for checking server version & 
        /// </summary>
        [DataMember]
        public string SteamApiKey { get; set; }

        /// <summary>
        /// Username to use to authenticate with SteamCMD. If you use 'anonymous' then password can be empty
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Password for above account. Can be empty if username is 'anonymous'
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Password for above account. Can be empty if username is 'anonymous'
        /// </summary>
        [DataMember]
        public int ServerUpdateTimeSeconds { get; set; }

        /// <summary>
        /// For now it will only manage one server, later may be able to have it manage multiple
        /// </summary>
        [DataMember]
        public SteamServerInfo ServerInfo { get; set; }

        /// <summary>
        /// Fail & exit if we are unable to kill the server or update the server. Some sevrers may still run fine if we are unable
        /// to update, however many require the latest version. For such servers FailOnException should be set to true
        /// </summary>
        [DataMember]
        public bool FailOnException { get; set; }
    }
}
