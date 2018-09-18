using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETJump.ChatBot.Core;
using ETJump.ChatBot.Core.Context;
using ETJump.ChatBot.Core.Messages;
using ETJump.ChatBot.Core.Plugins;
using ETJump.ChatBot.Plugin.ChatLinker.Settings;
using Newtonsoft.Json;

namespace ETJump.ChatBot.Plugin.ChatLinker
{
    public class ChatLinkerPlugin : IPlugin
    {
        private IContextV1 _context;
        private ChatLinkerPluginSettings _settings;
        public string Name { get; } = "Chat linker";

        public Task InitializeAsync(IContextV1 context)
        {
            _context = context;
            _settings = JsonConvert.DeserializeObject<ChatLinkerPluginSettings>(
                File.ReadAllText("chatlinker.plugin.config.json"));
            return Task.CompletedTask;
        }

        public async void MessageReceived(IMessageV1 message)
        {
            var tasks = new List<Task>();

            if (_settings.Links.ContainsKey(message.Server.Name))
            {
                var linkedServerNames = _settings.Links[message.Server.Name];

                var linkedServers = _context.Servers.Where(s => linkedServerNames.Contains(s.Name));

                foreach (var server in linkedServers)
                {
                    tasks.Add(server.SendMessageAsync($"{message.Sender}^7@{server.DisplayName}^7", message.Message));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
