using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace ETJump.ChatBot.Core.Settings
{
    public class Settings
    {
        public List<ServerSettings> Servers { get; set; }
    }
}
