using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace ETJump.ChatBot.Core.Context
{
    public interface IContextV1
    {
        ILogger Logger { get;set; }
        List<Server> Servers { get; set; }
    }
}
