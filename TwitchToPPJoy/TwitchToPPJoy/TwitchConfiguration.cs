using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TwitchToPPJoy
{
    public class TwitchConfiguration : PropertyChangedBase, IXSerializable
    {
        private string username;
        private string password;
        private BindableCollection<TwitchGamepad> gamepads;

        public TwitchConfiguration()
        {
            this.Gamepads = new BindableCollection<TwitchGamepad>();
        }

        public TwitchConfiguration(XElement element)
        {
            this.Gamepads = new BindableCollection<TwitchGamepad>();

            this.FromXElement(element);
        }

        public string Username
        {
            get
            {
                return this.username;
            }
            set
            {
                if (this.username != value)
                {
                    this.username = value;
                    NotifyOfPropertyChange(() => this.Username);
                }
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                if (this.password != value)
                {
                    this.password = value;
                    NotifyOfPropertyChange(() => this.Password);
                }
            }
        }

        public BindableCollection<TwitchGamepad> Gamepads
        {
            get
            {
                return this.gamepads;
            }
            set
            {
                if (this.gamepads != value)
                {
                    this.gamepads = value;
                    NotifyOfPropertyChange(() => this.Gamepads);
                }
            }
        }

        public XElement ToXElement()
        {
            XElement element = new XElement("Configuration");

            element.Add(new XAttribute("Username", this.Username));
            element.Add(new XAttribute("Password", this.Password));

            foreach (TwitchGamepad gamepad in this.Gamepads)
            {
                element.Add(gamepad.ToXElement());
            }

            return element;
        }

        public void FromXElement(XElement element)
        {            
            this.Username = element.Attribute("Username").Value;
            this.Password = element.Attribute("Password").Value;

            foreach(XElement gamepadElement in element.Elements("TwitchGamepad"))
            {
                this.Gamepads.Add(new TwitchGamepad(gamepadElement));
            }
        }
    }
}
