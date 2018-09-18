using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ETJump.ChatBot.Core.Context;
using ETJump.ChatBot.Core.Messages;
using ETJump.ChatBot.Core.Plugins;
using ETJump.ChatBot.Plugin.Discord.Settings;
using Newtonsoft.Json;

namespace ETJump.ChatBot.Plugin.Discord
{
    public class DiscordPlugin : IPlugin
    {
        private IContextV1 _context;
        private DiscordSocketClient _client;
        private bool _ready;
        private DiscordPluginSettings _settings;
        public string Name { get; } = "Discord";
        private const string ConfigFile = "discord.plugin.config.json";

        public async Task InitializeAsync(IContextV1 context)
        {
            _context = context;

            _context.Logger.Info("Starting plugin {Plugin}", Name);
            _context.Logger.Debug("Loading configuration {ConfigFile}", ConfigFile);

            // let jsonconvert throw if this fails --> app will unload plugin
            _settings = JsonConvert.DeserializeObject<DiscordPluginSettings>(File.ReadAllText(ConfigFile));

            _client = new DiscordSocketClient();

            await _client.LoginAsync(TokenType.Bot, _settings.Token);
            await _client.StartAsync();

            _client.Ready += () =>
            {
                _context.Logger.Info("Plugin {Plugin} has been initialized", Name);
                _ready = true;

                foreach (var clientGuild in _client.Guilds)
                {
                    _context.Logger.Info("Guild {GuildName} ({GuildId}) contains {ChannelCount} channels", clientGuild.Name, clientGuild.Id, clientGuild.Channels.Count);
                    
                    foreach (var clientGuildChannel in clientGuild.Channels)
                    {
                        _context.Logger.Info("Channel {ChannelName} ({ChannelId})", clientGuildChannel.Name, clientGuildChannel.Id);
                    }
                }

                return Task.CompletedTask;
            };

            _client.MessageReceived += async (message) =>
            {
                foreach (var link in _settings.ChannelServerLinks.Where(l => l.ChannelId == message.Channel.Id))
                {
                    var server = _context.Servers.FirstOrDefault(s => s.Name == link.Server);
                    if (server != null)
                    {
                        await server.SendMessageAsync(
                            $"{message.Author.Username}@{message.Channel.Name}",
                            message.Content
                        );
                    }
                }
            };
        }

        public void MessageReceived(IMessageV1 message)
        {
            if (!_ready)
            {
                return;
            }

            foreach (var link in _settings.ChannelServerLinks.Where(l => l.Server == message.Server.Name))
            {
                _client.GetGuild(link.GuildId).GetTextChannel(link.ChannelId)
                    .SendMessageAsync($"{message.Sender}@{message.Server.DisplayName}: {message.Message}");
            }
        }
    }
}
