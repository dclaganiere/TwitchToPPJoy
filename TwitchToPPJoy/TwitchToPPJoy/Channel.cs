using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchToPPJoy
{
    public class Channel : PropertyChangedBase
    {
        private string channelName;

        public Channel()
        {
        }

        public Channel(string channelName)
        {
            this.channelName = channelName;
        }

        public string ChannelName
        {
            get
            {
                return this.channelName;
            }
            set
            {
                if(this.channelName != value)
                {
                    this.channelName = value;
                    NotifyOfPropertyChange(() => this.ChannelName);
                }
            }
        }
    }
}
