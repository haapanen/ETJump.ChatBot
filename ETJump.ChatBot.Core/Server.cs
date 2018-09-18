using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ETJump.ChatBot.Core.ServerCommunication;
using ETJump.ChatBot.Core.Settings;

namespace ETJump.ChatBot.Core
{
    public class Server : IDisposable
    {
        private const int ServerThrottleTime = 500;

        private ConcurrentQueue<(string Source, string Message)> _buffer = new ConcurrentQueue<(string Source, string Message)>();

        private Timer _timer;

        private readonly IClient _client;
        public Server(IClient client)
        {
            _client = client;
            _timer = new Timer(async (state) =>
            {
                if (_buffer.TryDequeue(out var next))
                {
                    await _client.SendChatMessageAsync(next.Source, next.Message);
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(ServerThrottleTime + 10));
        }

        public Task SendMessageAsync(string source, string message)
        {
            if (_buffer.Any())
            {
                _buffer.Enqueue((source, message));
                return Task.CompletedTask;
            }
            else
            {
                return _client.SendChatMessageAsync(source, message);
            }
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Hostname { get; set; }
        public ushort Port { get; set; }
        public ServerSettings Settings { get; set; }

        public void Dispose()
        {
           
        }
    }
}
