using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System;

namespace RandomClassSelector
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        //public string Username = "Your twitch.tv username";
        //public string ChannelToSend = "Channel to send chat to";
        //public string OAuthCode = "";
        //public bool TwitchEnabled = true;
        public int MaxClassLevel = 100;
        public bool PrintAllChoices = false;
        public bool ChangeGSUsingShortname = false;
        public bool ChangeGSUsingLongname = false;
        public bool SuggestBLU = false;
        public bool SuggestCraftersGatherers = false;

        private IDalamudPluginInterface pluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
