using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ETJump.ChatBot.Core;
using ETJump.ChatBot.Core.Extensions;
using ETJump.ChatBot.Core.ServerCommunication;

namespace ETJump.Communication
{
    public class Client : IClient
    {
        private readonly string _rconPassword;
        private readonly UdpClient _udpClient;
        private readonly Encoder _encoder;
        private readonly byte[] _prefix = {0xff, 0xff, 0xff, 0xff};

        public Client(string hostname, ushort port, string rconPassword)
        {
            _rconPassword = rconPassword;
            _udpClient = new UdpClient(hostname, port);
            _encoder = new Encoder();
        }

        public async Task<StatusResponse> GetStatusAsync(TimeSpan timeout)
        {
            var packet = CreatePacket("getstatus");

            await _udpClient.SendAsync(packet, packet.Length);

            var responseBytes = await _udpClient.ReceiveAsync().WithTimeout(timeout);

            var stringResponse = Encoding.ASCII.GetString(responseBytes.Buffer);

            var lines = stringResponse.Replace("\r\n", "\n").Split('\n');
            var keyValues = lines[1].Split('\\');

            string key = null;
            var statusResponse = new StatusResponse
            {
                Settings = new Dictionary<string, string>()
            };
            foreach (var iter in keyValues.Skip(1))
            {
                if (key == null)
                {
                    key = iter;
                }
                else
                {
                    statusResponse.Settings[key] = iter;
                    key = null;
                }
            }

            return statusResponse;
        }

        public async Task<bool> TestRconPasswordAsync(TimeSpan timeout)
        {
            try
            {
                return await TestRconPasswordAsync().WithTimeout(timeout);
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        private async Task<bool> TestRconPasswordAsync()
        {
            var packet = CreatePacket($"rcon {_rconPassword} status");
            await _udpClient.SendAsync(packet, packet.Length);
            var response = await _udpClient.ReceiveAsync();
            return !Encoding.ASCII.GetString(response.Buffer.Skip(4).ToArray()).StartsWith("print\nBad rconpassword.");
        }

        public async Task SendChatMessageAsync(string sender, string message)
        {
            var packet = CreatePacket($"rcon {_rconPassword} enc_qsay \"{_encoder.Encode($"{sender}: {message}")}\"");

            await _udpClient.SendAsync(packet, packet.Length);
        }

        private byte[] CreatePacket(string input)
        {
            return _prefix.Concat(Encoding.ASCII.GetBytes(input)).ToArray();
        }

        public void Dispose()
        {
            _udpClient.Close();
        }
    }
}
