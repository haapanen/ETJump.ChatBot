using System;
using System.Collections.Generic;
using System.Text;

namespace ETJump.ChatBot.Plugin.Discord.Settings
{
    public class DiscordPluginSettings
    {
        public string Token { get; set; }
        public List<ChannelServerLinkSetting> ChannelServerLinks { get; set; }
    }
}
