using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchToPPJoy;
using TwitchToPPJoyUI.Models;

namespace TwitchToPPJoyUI.ViewModels
{
    [Export(typeof(GamepadViewModel))]
    public class GamepadViewModel : PropertyChangedBase
    {
        private TwitchGamepad gamepad;
        private BindableCollection<TwitchInput> inputs;
        private string message;
        private int value;

        [ImportingConstructor]
        public GamepadViewModel(TwitchGamepad gamepad)
        {
            this.gamepad = gamepad;
            this.inputs = new BindableCollection<TwitchInput>();

            foreach(var kvp in gamepad.Inputs)
            {
                this.inputs.Add(new TwitchInput(this.gamepad.Inputs, kvp.Key, kvp.Value));
            }
        }

        public string Name
        {
            get
            {
                return this.gamepad.Name;
            }
            set
            {
                if (this.gamepad.Name != value)
                {
                    this.gamepad.Name = value;
                    NotifyOfPropertyChange(() => this.Name);
                }
            }
        }

        public int Controller
        {
            get
            {
                return this.gamepad.Joystick.VirtualStickNumber;
            }
            set
            {
                if (this.gamepad.Joystick.VirtualStickNumber != value)
                {
                    this.gamepad.ConfigureController(value);
                    NotifyOfPropertyChange(() => this.Controller);
                }
            }
        }

        public int PressLength
        {
            get
            {
                return this.gamepad.PressLength;
            }
            set
            {
                if (this.gamepad.PressLength != value)
                {
                    this.gamepad.PressLength = value;
                    NotifyOfPropertyChange(() => this.PressLength);
                }
            }
        }

        public BindableCollection<Channel> Channels
        {
            get
            {
                return this.gamepad.Channels;
            }
            set
            {
                if (this.gamepad.Channels != value)
                {
                    this.gamepad.Channels = value;
                    NotifyOfPropertyChange(() => this.Channels);
                }
            }
        }

        public BindableCollection<TwitchInput> Inputs
        {
            get
            {
                return this.inputs;
            }
            set
            {
                if (this.inputs != value)
                {
                    this.inputs = value;
                    NotifyOfPropertyChange(() => this.Inputs);
                }
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                if (this.message != value)
                {
                    this.message = value;
                    NotifyOfPropertyChange(() => this.Message);
                    NotifyOfPropertyChange(() => this.CanAddInput);
                }
            }
        }
        public int Value
        {
            get
            {
                return this.value;
            }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    NotifyOfPropertyChange(() => this.Value);
                    NotifyOfPropertyChange(() => this.CanAddInput);
                }
            }
        }

        public bool CanAddInput
        {
            get
            {
                return this.Message != null && !this.gamepad.Inputs.ContainsKey(this.Message);
            }
        }

        public void AddChannel(string channel)
        {
            this.Channels.Add(new Channel(channel));
        }

        public void DeleteChannel(Channel channel)
        {
            this.Channels.Remove(channel);
        }

        public void AddInput()
        {
            this.Inputs.Add(new TwitchInput(this.gamepad.Inputs, this.Message, this.Value));
            this.gamepad.Inputs.Add(this.Message, this.Value);
            this.Message = null;
            this.Value = 0;
        }

        public void DeleteInput(TwitchInput input)
        {
            this.gamepad.Inputs.Remove(input.Message);
            this.Inputs.Remove(input);
        }
    }
}
