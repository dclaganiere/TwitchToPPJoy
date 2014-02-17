using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchClient
{
    public interface IMessageHandler
    {
        void HandleMessage(IrcMessage ircMessage);
    }
}
