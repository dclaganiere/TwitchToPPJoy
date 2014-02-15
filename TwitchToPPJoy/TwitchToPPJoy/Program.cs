using PPJoy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchToPPJoy.Properties;
using TwitchClient;
using System.Threading;

namespace TwitchToPPJoy
{
    class Program
    {
        private static Dictionary<string, int> inputs = new Dictionary<string, int>();
        private static VirtualJoystick joystick;

        private static void Main(string[] args)
        {
            // Create a client.
            TwitchIRCClient client = new TwitchIRCClient(
                Settings.Default.Username,
                Settings.Default.Password,
                Settings.Default.Channel);

            // Add Handler for messages.
            client.OnMessageRecieved += client_OnMessageRecieved;

            // Parse PPJoy bindings.
            BindInputs();

            // Setup controller device.
            ConfigureController();

            // Start listening.
            client.ConnectAndListenForMessages();

            // TODO: Stop listening
        }

        static void client_OnMessageRecieved(string username, string message)
        {
            int code;
            string messageLower = message.ToLowerInvariant();

            // Check if message is an input.
            if (inputs.TryGetValue(messageLower, out code))
            {
                // Enable button.
                joystick.SetDigitalDataSourceState(code, true);
                joystick.SendUpdates();

                Thread.Sleep(50);

                // Disable button.
                joystick.SetDigitalDataSourceState(code, false);
                joystick.SendUpdates();

                Console.WriteLine("{0,-26} {1}", username + ":", messageLower);
            }
        }

        private static void BindInputs()
        {
            foreach (string kvp in Settings.Default.Inputs)
            {
                // Split pair into parts.
                string[] keys = kvp.Split(' ');

                // Add input to list.
                string key = keys[0].ToLowerInvariant();
                int value = int.Parse(keys[1]);
                inputs.Add(key, value);
            }
        }

        private static void ConfigureController()
        {
            int controller = Settings.Default.Controller;
            DeviceManager manager = new DeviceManager();

            // Make sure device exists.
            Device device = manager.GetDevice(controller, 0);
            if (device == null)
            {
                manager.CreateDevice(controller, JoystickTypes.Virtual_Joystick, JoystickSubTypes.NotApplicable, 0);
            }

            // Set up virtual joystick.
            joystick = new VirtualJoystick(controller);
        }
    }
}
