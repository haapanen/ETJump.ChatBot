using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ETJump.ChatBot.Core;
using ETJump.ChatBot.Core.Context.Implementation;
using ETJump.ChatBot.Core.Messages.Implementation;
using ETJump.ChatBot.Core.Plugins;
using ETJump.ChatBot.Core.ServerCommunication;
using ETJump.ChatBot.Core.Settings;
using ETJump.Communication;
using NLog;

namespace ETJump.ChatBot
{
    public class Application
    {
        private class ServerHandles
        {
            public Server Server { get; set; }
            public FileStream Stream { get; set; }
            public StreamReader Reader { get; set; }
        }

        private readonly ILogger _logger;
        private readonly Settings _settings;

        public Application(ILogger logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public async Task RunAsync(string[] args)
        {
            _logger.Info("Starting application");

            _servers = await BuildServersAsync(_settings);
            
            _plugins = await LoadPluginsAsync();

            foreach (var server in _settings.Servers.Where(s =>
                string.IsNullOrEmpty(s.LogFile) || !File.Exists(s.LogFile)))
            {
                _logger.Info("Skipping {Name}, no log file available", server.Name);
            }

            var polledServers =
                _settings.Servers.Where(s => !string.IsNullOrEmpty(s.LogFile) && File.Exists(s.LogFile))
                    .ToList();
            foreach (var server in polledServers)
            {
                _logger.Info("Polling server {DisplayName} ({Name})", server.DisplayName, server.Name);
            }

            var serverHandles = new List<ServerHandles>();

            foreach (var server in polledServers)
            {
                var stream = new FileStream(server.LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var reader = new StreamReader(stream);

                stream.Seek(0, SeekOrigin.End);

                serverHandles.Add(new ServerHandles
                {
                    Server = _servers.FirstOrDefault(s => s.Name == server.Name),
                    Stream = stream,
                    Reader = reader
                });
            }

            while (true)
            {
                Thread.Sleep(250);

                foreach (var server in serverHandles)
                {
                    if (server.Reader.EndOfStream)
                    {
                        continue;
                    }

                    while (!server.Reader.EndOfStream)
                    {
                        var line = server.Reader.ReadLine();

                        HandleLine(server.Server, line);
                    }
                }
            }

            foreach (var handles in serverHandles)
            {
                handles.Reader.Close();
                handles.Stream.Close();
            }
            //using (var logFile = new FileStream("F:\\ETJump\\et\\etjump\\etconsole.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //using (var reader = new StreamReader(logFile))
            //{
            //    logFile.Seek(0, SeekOrigin.End);

            //    var previousLength = 0;

            //    while (true)
            //    {
            //        Thread.Sleep(100);

            //        if (reader.EndOfStream)
            //        {
            //            continue;
            //        }

            //        while (!reader.EndOfStream)
            //        {
            //            var line = reader.ReadLine();

            //        }
            //    }
            //}
        }

        private async Task<List<Server>> BuildServersAsync(Settings settings)
        {
            var servers = new List<Server>();

            foreach (var server in settings.Servers)
            {
                if (string.IsNullOrEmpty(server.IpAddress))
                {
                    _logger.Warn("No IP address specified for server {ServerName}. Skipping server.", server.Name);
                    continue;
                }

                var ipPort = server.IpAddress.Split(":");

                string hostname = null;
                ushort port = 27960;
                hostname = ipPort[0];
                if (ipPort.Length > 1)
                {
                    if (!ushort.TryParse(ipPort[1], out port) || port < 1)
                    {
                        _logger.Warn("Invalid port {Port} specified for server {ServerName}. Skipping server.",
                            ipPort[1], server.Name);
                        continue;
                    }
                }

                Client client = null;

                try
                {
                    client = new Client(hostname, port, server.RconPassword);
                }
                catch (SocketException)
                {
                    _logger.Warn("Server {Hostname}:{Port} is unknown. Make sure the IP address or domain name is valid.", hostname, port);
                    continue;
                }

                string displayName = server.DisplayName;
                try
                {
                    var serverStatus = await client.GetStatusAsync(TimeSpan.FromSeconds(2));
                    displayName = serverStatus.Settings["sv_hostname"];
                    _logger.Info("Found sv_hostname for server {Server}: {DisplayName}", server.Name, displayName);
                    if (!await client.TestRconPasswordAsync(TimeSpan.FromSeconds(2)))
                    {
                        _logger.Warn("Bad rcon password {RconPassword} for server {Server}", server.RconPassword, server.Name);
                    }
                }
                catch (Exception e)
                {
                    switch (e)
                    {
                        case SocketException se:
                        case TimeoutException te:
                            _logger.Warn("Server {Hostname}:{Port} is unreachable. Using {DisplayName} as name.", hostname, port, server.DisplayName);
                            break;
                        default:
                            _logger.Warn("Unknown exception while trying to fetch display name for server {Hostname}:{Port}. Skipping server.", hostname, port);
                            continue;
                    }
                }

                servers.Add(new Server(client)
                {
                    Name = server.Name,
                    DisplayName = displayName,
                    Hostname = hostname,
                    Port = port,
                    Settings = server,
                });
            }


            return servers;
        }

        private async Task<List<IPlugin>> LoadPluginsAsync()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            var plugins = new List<IPlugin>();
            foreach (var dll in Directory.EnumerateFiles(currentDirectory, "*.dll"))
            {
                var asm = Assembly.LoadFrom(dll);
                foreach (var pluginType in asm.GetExportedTypes()
                    .Where(t => t.IsClass && typeof(IPlugin).IsAssignableFrom(t)))
                {
                    var plugin = (IPlugin) Activator.CreateInstance(pluginType);

                    _logger.Debug("Found plugin {PluginName}", plugin.Name);

                    try
                    {
                        await plugin.InitializeAsync(new Context
                        {
                            // FIXME: inject a factory and use it?
                            Logger = LogManager.GetLogger(pluginType.FullName),
                            Servers = _servers
                        });
                        plugins.Add(plugin);
                    }
                    catch (Exception exception)
                    {
                        _logger.Error("Uncaught exception from plugin {Plugin}. Unloading plugin.", plugin.Name, exception);
                    }
                }
            }

            return plugins;
        }

        private readonly Regex _allChatPattern = new Regex("^say: (.+): (.+)$");
        private readonly Regex _teamChatPattern = new Regex("^sayteam: (.+): (.+)$");
        private List<IPlugin> _plugins;
        private List<Server> _servers;

        private void HandleLine(Server server, string line)
        {
            if (server == null)
            {
                return;
            }

            if (HandleChat(server, line))
            {
                return;
            }
        }

        private bool HandleChat(Server server, string line)
        {
            Match match = null;
            string name = null;
            string message = null;

            if (line.StartsWith("say: "))
            {
                match = _allChatPattern.Match(line);
                if (match.Success)
                {
                    name = match.Groups[1].Value;
                    message = match.Groups[2].Value;

                    _logger.Debug("All chat message at {Server} from {Player}: {Message}", server.Name, name, message);
                }
            }
            else if (line.StartsWith("sayteam: "))
            {
                match = _teamChatPattern.Match(line);
                if (match.Success)
                {
                    name = match.Groups[1].Value;
                    message = match.Groups[2].Value;

                    _logger.Debug("Team chat message at {Server} from {Player}: {Message}", server.Name, name, message);
                }
            }
            else
            {
                return false;
            }

            var faultyPlugins = new List<IPlugin>();

            foreach (var plugin in _plugins)
            {
                try
                {
                    plugin.MessageReceived(new MessageV1(server, name, message));
                }
                catch (Exception e)
                {
                    _logger.Error("Unloading plugin {Plugin} due to uncaught exception", plugin.Name, e);
                    faultyPlugins.Add(plugin);
                }
            }

            foreach (var plugin in faultyPlugins)
            {
                _plugins.Remove(plugin);
            }

            return true;
        }
    }
}