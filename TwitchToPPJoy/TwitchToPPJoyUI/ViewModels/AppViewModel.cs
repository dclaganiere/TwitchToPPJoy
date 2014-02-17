using Caliburn.Micro;
using Microsoft.Win32;
using PPJoy;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using TwitchClient;
using TwitchToPPJoy;

namespace TwitchToPPJoyUI.ViewModels
{
    [Export(typeof(AppViewModel))]
    public class AppViewModel : PropertyChangedBase
    {
        private readonly IWindowManager windowManager;
        private TwitchConfiguration configuration;

        [ImportingConstructor]
        public AppViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            this.configuration = new TwitchConfiguration();
        }

        public string Username
        {
            get
            {
                return this.configuration.Username;
            }
            set
            {
                if (this.configuration.Username != value)
                {
                    this.configuration.Username = value;
                    NotifyOfPropertyChange(() => Username);
                }
            }
        }
        
        public string Password
        {
            get
            {
                return this.configuration.Password;
            }
            set
            {
                if (this.configuration.Password != value)
                {
                    this.configuration.Password = value;
                    NotifyOfPropertyChange(() => Password);
                }
            }
        }

        public BindableCollection<TwitchGamepad> Gamepads
        {
            get
            {
                return this.configuration.Gamepads;
            }
        }

        public void AddGamepad()
        {
            TwitchGamepad gamepad = new TwitchGamepad();
            gamepad.Name = "New Gamepad";
            Gamepads.Add(gamepad);

            EditGamepad(gamepad);
        }

        public void EditGamepad(TwitchGamepad gamepad)
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            settings.Title = "Edit Gamepad";

            windowManager.ShowWindow(new GamepadViewModel(gamepad), null, settings);

        }

        public void DeleteGamepad(TwitchGamepad gamepad)
        {
            Gamepads.Remove(gamepad);
        }
        
        public void SaveConfiguration()
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.AddExtension = true;
            dialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";

            if(dialog.ShowDialog() == true)
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create(dialog.FileName, xmlWriterSettings))
                {
                    XDocument document = new XDocument();
                    document.Add(this.configuration.ToXElement());
                    document.WriteTo(writer);
                }
            }
        }

        public void LoadConfiguration()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";

            if(dialog.ShowDialog() == true)
            {
                XDocument test = XDocument.Load(dialog.FileName);

                this.configuration = new TwitchConfiguration(test.Element("Configuration"));

                NotifyOfPropertyChange(() => this.Username);
                NotifyOfPropertyChange(() => this.Password);
                NotifyOfPropertyChange(() => this.Gamepads);
            }
        }

        private TwitchIRCClient client;
        public void StartClient()
        {
            var channels = new List<string>();

            foreach(TwitchGamepad gamepad in this.configuration.Gamepads)
            {
                channels.AddRange(gamepad.Channels.Select(o => o.ChannelName));
            }

            this.client = new TwitchIRCClient(this.Username, this.Password, channels);
            this.client.ConnectAndListenForMessages();

#if DEBUG
            this.client.MessageStream.Subscribe(DebugHandleMessage);
#endif
       
            foreach(TwitchGamepad gamepad in this.configuration.Gamepads)
            {
                TwitchToPPJoyController controller = new TwitchToPPJoyController(gamepad, client);
                this.ShowMessages(gamepad, client);
            }
        }

        public void DebugHandleMessage(string message)
        {
            Console.WriteLine(message);
        }
        public void ShowMessages(TwitchGamepad gamepad, TwitchIRCClient client)
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            settings.Title = gamepad.Name;

            var channelHash = new HashSet<string>();

            foreach (Channel channel in gamepad.Channels)
            {
                channelHash.Add(channel.ChannelName);
            }

            windowManager.ShowWindow(new MessagesViewModel(channelHash, gamepad.Inputs, client), null, settings);

        }
    }
}
