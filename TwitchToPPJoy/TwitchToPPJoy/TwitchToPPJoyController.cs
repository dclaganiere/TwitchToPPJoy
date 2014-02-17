using PPJoy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchClient;

namespace TwitchToPPJoy
{
    public class TwitchToPPJoyController : IMessageHandler
    {
        private TwitchGamepad gamepad;
        private HashSet<string> channelHash;

        public TwitchToPPJoyController(TwitchGamepad gamepad, TwitchIRCClient client)
        {
            this.gamepad = gamepad;

            this.channelHash = new HashSet<string>();
            
            foreach(Channel channel in gamepad.Channels)
            {
                this.channelHash.Add(channel.ChannelName);
            }

            client.IrcMessageStream.SubscribeSafe(Observer.Create<IrcMessage>(HandleMessage));
        }

        public void HandleMessage(IrcMessage ircMessage)
        {
            if (this.channelHash.Contains(ircMessage.Channel))
            {
                int code;
                string messageLower = ircMessage.Message.ToLowerInvariant();

                // Check if message is an input.
                if (this.gamepad.Inputs.TryGetValue(messageLower, out code))
                {
                    // Enable button.
                    this.gamepad.Joystick.SetDigitalDataSourceState(code, true);
                    this.gamepad.Joystick.SendUpdates();

                    Thread.Sleep(this.gamepad.PressLength);

                    // Disable button.
                    this.gamepad.Joystick.SetDigitalDataSourceState(code, false);
                    this.gamepad.Joystick.SendUpdates();
                }
            }
        }
    }
}
