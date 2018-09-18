using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace ETJump.ChatBot.Core.Context.Implementation
{
    public class Context : IContextV1
    {
        public ILogger Logger { get; set; }
        public List<Server> Servers { get; set; }
    }
}
