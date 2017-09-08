using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace TreeChat
{
    public class TreeChat
    {
        private readonly UdpClient udpclient;
        private readonly IPEndPoint parentEndPoint;
        private readonly int lossPercent;

        private State state;

        private readonly ConcurrentBag<IPEndPoint> childs = new ConcurrentBag<IPEndPoint>();

        private readonly ConcurrentDictionary<Guid, Message> lastRecievedMessages =
            new ConcurrentDictionary<Guid, Message>();

        private readonly ConcurrentDictionary<Guid, Message> pendingMessages =
            new ConcurrentDictionary<Guid, Message>();

        public TreeChat(int localPort, IPEndPoint parentEndPoint, int lossPercent)
        {
            this.parentEndPoint = parentEndPoint;
            this.lossPercent = lossPercent;

            udpclient = new UdpClient(localPort);

            this.state = parentEndPoint == null ? State.Working : State.WaitingForParentConnection;
        }

        public void Start()
        {
        }

        private async Task SendLoop()
        {
            while (state == State.WaitingForParentConnection)
            {
                byte[] message = {(byte) CommandCode.ConnectToParent};
                await udpclient.SendAsync(message, message.Length, parentEndPoint).ConfigureAwait(false);

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        private async Task RecieveLoop()
        {
            while (true)
            {
                UdpReceiveResult data = await udpclient.ReceiveAsync().ConfigureAwait(false);

                if (new Random().Next(100) < lossPercent)
                {
                    // Simulate packet loss
                    continue;
                }

                switch ((CommandCode) data.Buffer[0])
                {
                    case CommandCode.ConnectToParent:
                        childs.Add(data.RemoteEndPoint);

                        byte[] message = {(byte) CommandCode.ConnectToParentAck};
                        await udpclient.SendAsync(message, message.Length, data.RemoteEndPoint).ConfigureAwait(false);

                        break;

                    case CommandCode.ConnectToParentAck:
                        if (!data.RemoteEndPoint.Equals(parentEndPoint))
                        {
                            Console.WriteLine($"Got ConnectToParentAck from {data.RemoteEndPoint}, ignoring");
                        }

                        if (state == State.WaitingForParentConnection)
                        {
                            state = State.Working;
                        }
                        break;

                    case CommandCode.Message:
                        await this.MessageRecieved(data).ConfigureAwait(false);
                        break;

                    case CommandCode.MessageAck:
                        byte[] guidBytes = new byte[data.Buffer.Length - 1];
                        Array.Copy(data.Buffer, 1, guidBytes, 0, guidBytes.Length);

                        Message m;
                        pendingMessages.TryRemove(new Guid(guidBytes), out m);
                        break;

                    case CommandCode.Dead:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task MessageRecieved(UdpReceiveResult data)
        {
            if (!data.RemoteEndPoint.Equals(parentEndPoint))
            {
                using (var ms = new MemoryStream(data.Buffer))
                {
                    var message = (Message) new BinaryFormatter().Deserialize(ms);
                }
            }
        }
    }
}
