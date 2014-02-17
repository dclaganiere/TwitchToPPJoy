using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchToPPJoyUI.Models
{
    public class TwitchInput : PropertyChangedBase
    {
        private Dictionary<string, int> inputs;
        private string message;
        private int value;

        public TwitchInput()
        {
        }

        public TwitchInput(Dictionary<string,int> inputs, string message, int value)
        {
            this.inputs = inputs;
            this.message = message;
            this.value = value;
        }

        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                if(this.message != value && value != null && !inputs.ContainsKey(value))
                {
                    inputs.Remove(this.message);
                    this.message = value;
                    inputs.Add(this.message, this.value);

                    NotifyOfPropertyChange(() => this.Message);
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
                if(this.value != value && this.message != null)
                {
                    this.value = value;
                    this.inputs[this.message] = this.value;
                    NotifyOfPropertyChange(() => this.Value);
                }
            }
        }
    }
}
