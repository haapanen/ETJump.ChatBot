using System;
using System.Collections.Generic;
using System.Text;
using ETJump.ChatBot.Core.ServerCommunication;

namespace ETJump.ChatBot.Core.Messages
{
    public interface IMessageV1
    {
        Server Server { get; }
        string Sender { get; }
        string Message { get; }
    }
}
