using ETJump.ChatBot.Core.ServerCommunication;

namespace ETJump.ChatBot.Core.Messages.Implementation
{
    public class MessageV1 : IMessageV1
    {
        public MessageV1(Server server, string sender, string message)
        {
            Server = server;
            Sender = sender;
            Message = message;
        }

        public Server Server { get; }
        public string Sender { get; }
        public string Message { get; }
    }
}
