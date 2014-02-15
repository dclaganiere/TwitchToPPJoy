using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchClient
{
    public class TwitchIRCClient
    {
        private const string SERVER = "irc.twitch.tv";
        private const int PORT = 6667;

        private static readonly char[] DELIM = new char[] { ' ' };


        private string username;
        private string password;
        private string channel;

        private TcpClient client;

        private CancellationTokenSource tokenSource;

        public TwitchIRCClient(string username, string password, string channel)
        {
            this.username = username;
            this.password = password;
            this.channel = channel;

        }

        public delegate void MessageHandler(string username, string message);
        public event MessageHandler OnMessageRecieved;

        public void ConnectAndListenForMessages()
        {
            string buffer;

            this.tokenSource = new CancellationTokenSource();
            this.client = new TcpClient();

            //Connect to irc server and get input and output text streams from TcpClient.
            this.client.Connect(SERVER, PORT);

            if (!this.client.Connected)
            {
                Console.WriteLine("Failed to connect!");
                return;
            }

            using (TextReader input = new StreamReader(client.GetStream()))
            using (TextWriter output = new StreamWriter(client.GetStream()))
            {

                // Login to server.
                output.Write("PASS {0}{1}", this.password, output.NewLine);
                output.Write("NICK {0}{1}", this.username, output.NewLine);
                output.Write("USER {0} 0 * :...{1}", this.username, output.NewLine);
                output.Flush();

                // Process each line received from irc server.
                while(!this.tokenSource.Token.IsCancellationRequested)
                {
                    buffer = input.ReadLine();

                    // Display received irc message
                    //Console.WriteLine(buffer);

                    // Send pong reply to any ping messages
                    if (buffer.StartsWith("PING "))
                    {
                        output.Write(buffer.Replace("PING", "PONG") + output.NewLine);
                        output.Flush();
                    }

                    // All messages should start with ':'.
                    if (buffer[0] != ':')
                    {
                        continue;
                    }

                    // After server sends 001 command, we can set mode to bot and join a channel.
                    if (buffer.Split(DELIM)[1] == "001")
                    {
                        output.Write("MODE {0} +B{2}JOIN {1}{2}", this.username, this.channel, output.NewLine);
                        output.Flush();
                    }

                    // Read in messages.
                    string[] data = buffer.Split(DELIM, 4);

                    if (data.Length == 4)
                    {
                        if (data[1] == "PRIVMSG")
                        {
                            int nameLength = data[0].IndexOf('!') - 1;

                            // This is used to ignore messages from jtv.
                            if (nameLength == -1)
                            {
                                continue;
                            }

                            string user = data[0].Substring(1, nameLength);
                            string message = data[3].Substring(1);

                            OnMessageRecieved.Invoke(user, message);

                        }
                    }
                }
            }

        }
        public void StopListening()
        {
            if (this.tokenSource != null)
            {
                this.tokenSource.Cancel();
            }
        }
    }
}
