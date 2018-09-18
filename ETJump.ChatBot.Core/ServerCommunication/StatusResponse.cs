using System.Collections.Generic;
using ETJump.ChatBot.Core.Extensions;

namespace ETJump.ChatBot.Core.ServerCommunication
{
    public class StatusResponse
    {
        public Dictionary<string, string> Settings { get; set; }

        public string Hostname => Settings.GetOrDefault("sv_hostname") ?? "";
    }
}