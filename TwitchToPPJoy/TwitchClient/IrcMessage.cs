using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchClient
{
    public class IrcMessage
    {
        public IrcMessage(string channel, string username, string message)
        {
            this.Channel = channel;
            this.Username = username;
            this.Message = message;
        }

        public string Username { get; set; }
        
        public string Channel { get; set; }
        
        public string Message { get; set; }
    }
}
