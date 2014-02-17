using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchClient
{
    public class TwitchIRCClient : IDisposable
    {
        private const string SERVER = "irc.twitch.tv";
        private const int PORT = 6667;

        private static readonly char[] DELIM = new char[] { ' ' };


        private string username;
        private string password;
        private List<string> channels;

        private TcpClient client;
        private TextWriter writer;

        private Subject<string> messageStream;
        private Subject<IrcMessage> ircMessageStream;

        public TwitchIRCClient(string username, string password, List<string> channels)
        {
            this.username = username;
            this.password = password;
            this.channels = new List<string>();

            this.channels.AddRange(channels);

        }

        public IObservable<string> MessageStream
        {
            get
            {
                return this.messageStream.AsObservable();
            }
        }

        public IObservable<IrcMessage> IrcMessageStream
        {
            get
            {
                return this.ircMessageStream;
            }
        }

        public IEnumerable<string> Channels
        {
            get
            {
                return channels.AsReadOnly();
            }
        }

        public void ConnectAndListenForMessages()
        {
            // Create a new client.
            this.client = new TcpClient();

            //Connect to irc server and get input and output text streams from TcpClient.
            this.client.Connect(SERVER, PORT);

            if (!this.client.Connected)
            {
                Console.WriteLine("Failed to connect!");
                return;
            }

            TextReader input = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());


            // Process each line received from irc server.
            this.messageStream = new Subject<string>();
            this.ircMessageStream = new Subject<IrcMessage>();

            messageStream.Subscribe(HandleMessage);

            Task.Run(() => GetMessages(input));


            // Login to server.
            this.LogIn();

        }

        private void GetMessages(TextReader reader)
        {
            string buffer;
            while(true)
            {
                buffer = reader.ReadLine();
                messageStream.OnNext(buffer);
            }
        }

        private void HandleMessage(string buffer)
        {
            // Send pong reply to any ping messages
            if (buffer.StartsWith("PING "))
            {
                this.writer.Write(buffer.Replace("PING", "PONG") + this.writer.NewLine);
                this.writer.Flush();
                return;
            }

            // All messages should start with ':'.
            if (buffer[0] != ':')
            {
                return;
            }

            // After server sends 001 command, we can set mode to bot and join channels.
            if (buffer.Split(DELIM)[1] == "001")
            {
                this.JoinChannels();
            }

            // Read in messages.
            string[] data = buffer.Split(DELIM, 4);

            if (data.Length == 4)
            {
                if (data[1] == "PRIVMSG")
                {
                    int nameLength = data[0].IndexOf('!') - 1;

                    // This is used to ignore messages from jtv.
                    if (nameLength < 1)
                    {
                        return;
                    }

                    string channel = data[2];
                    string username = data[0].Substring(1, nameLength);
                    string message = data[3].Substring(1);

                    this.ircMessageStream.OnNext(new IrcMessage(channel, username, message));
                }
            }

            return;
        }

        private void JoinChannels()
        {
            //this.writer.Write("MODE {0}{1}", this.username, this.writer.NewLine);

            foreach(string channel in this.channels)
            {
                this.writer.Write("JOIN {0}{1}", channel, this.writer.NewLine);
            }

            this.writer.Flush();
        }

        private void LogIn()
        {
            this.writer.Write("PASS {0}{1}", this.password, this.writer.NewLine);
            this.writer.Write("NICK {0}{1}", this.username, this.writer.NewLine);
            this.writer.Write("USER {0} 0 * :...{1}", this.username, this.writer.NewLine);
            this.writer.Flush();
        }

        public void Dispose()
        {
            this.writer.Dispose();
        }
    }
}
