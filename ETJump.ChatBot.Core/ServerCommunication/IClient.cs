using System;
using System.Threading.Tasks;

namespace ETJump.ChatBot.Core.ServerCommunication
{
    public interface IClient : IDisposable
    {
        Task<StatusResponse> GetStatusAsync(TimeSpan timeout);
        Task<bool> TestRconPasswordAsync(TimeSpan timeout);
        /// <summary>
        /// Sends a message to server but doesn't guarantee it'll arrive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendChatMessageAsync(string sender, string message);
    }
}
