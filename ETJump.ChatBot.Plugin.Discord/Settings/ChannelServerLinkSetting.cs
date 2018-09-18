using System;
using System.Collections.Generic;
using System.Text;

namespace ETJump.ChatBot.Plugin.Discord.Settings
{
    public class ChannelServerLinkSetting
    {
        public string Server { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
