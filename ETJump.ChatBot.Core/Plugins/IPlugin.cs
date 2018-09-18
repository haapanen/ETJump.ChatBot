using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ETJump.ChatBot.Core.Context;
using ETJump.ChatBot.Core.Messages;

namespace ETJump.ChatBot.Core.Plugins
{
    public interface IPlugin
    {
        string Name { get; }
        Task InitializeAsync(IContextV1 context);
        void MessageReceived(IMessageV1 message);
    }
}
