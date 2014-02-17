using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TwitchClient;

namespace TwitchToPPJoyUI.ViewModels
{
    [Export(typeof(MessagesViewModel))]
    public class MessagesViewModel : PropertyChangedBase, IMessageHandler
    {
        private HashSet<string> channels;
        private Dictionary<string, int> inputs;
        private BindableCollection<IrcMessage> messages;
        private LinkedList<IrcMessage> linkedMessages;

        [ImportingConstructor]
        public MessagesViewModel(HashSet<string> channels, Dictionary<string, int> inputs, TwitchIRCClient client)
        {
            this.channels = channels;
            this.inputs = inputs;
            this.messages = new BindableCollection<IrcMessage>();
            this.linkedMessages = new LinkedList<IrcMessage>();
            client.IrcMessageStream.Subscribe(HandleMessage);
        }

        public BindableCollection<IrcMessage> Messages
        {
            get
            {
                return this.messages;
            }
            set
            {
                if (this.messages != value)
                {
                    this.messages = value;
                    NotifyOfPropertyChange(() => this.Messages);
                }
            }
        }

        public void HandleMessage(IrcMessage ircMessage)
        {
            if (this.channels.Contains(ircMessage.Channel))
            {
                int code;
                string messageLower = ircMessage.Message.ToLowerInvariant();

                // Check if message is an input.
                if (this.inputs.TryGetValue(messageLower, out code))
                {
                    ircMessage.Message = messageLower;

                    if (this.messages.Count > 20)
                    {
                        messages.Remove(linkedMessages.First());
                        this.linkedMessages.RemoveFirst();
                    }

                    this.messages.Add(ircMessage);
                    this.linkedMessages.AddLast(ircMessage);
                }
            }
        }
    }
}
