using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace TreeChat
{
    public class TreeChat
    {
        private readonly TimeSpan retryTimeout = TimeSpan.FromSeconds(3);
        private const int attemtsBeforeBan = 5;

        private readonly UdpClient udpclient;
        private readonly string name;
        private readonly int lossPercent;
        private IPEndPoint parentEndPoint;
        private IPEndPoint parentsParentEndPoint;

        private readonly CancellationTokenSource source = new CancellationTokenSource();
        private Task sendTask;
        private Task receiveTask;

        private State state;

        private readonly ConcurrentDictionary<IPEndPoint, Peer> peers = new ConcurrentDictionary<IPEndPoint, Peer>();

        private readonly ConcurrentDictionary<Guid, Message> lastRecievedMessages =
            new ConcurrentDictionary<Guid, Message>();

        public TreeChat(string name, int lossPercent, IPEndPoint parentEndPoint, int localPort)
        {
            this.name = name;
            this.lossPercent = lossPercent;

            this.parentEndPoint = parentEndPoint;
            if (parentEndPoint == null)
            {
                this.state = State.Working;
            }
            else
            {
                this.state = State.WaitingForParentConnection;
                peers.TryAdd(parentEndPoint, new Peer());
            }

            udpclient = new UdpClient(localPort);
        }

        public void Start()
        {
            sendTask = Task.Run(async () =>
            {
                try
                {
                    await SendLoop();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"FATAL ERROR in send loop: {e}");
                }
            }, source.Token);

            receiveTask = Task.Run(async () =>
            {
                try
                {
                    await ReceiveLoop();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"FATAL ERROR in receive loop: {e}");
                }
            }, source.Token);
        }

        public void Stop()
        {
            source.Cancel();
            Task.WaitAll(sendTask, receiveTask);
        }

        public void SendMessage(string text)
        {
            var message = new Message(Guid.NewGuid())
            {
                SenderName = this.name,
                Text = text,
                Created = DateTime.Now
            };

            foreach (var peer in peers)
            {
                peer.Value.PendingMessagesLastSendAttempt.TryAdd(message, new MessageSentData(DateTime.Now));
                this.SendMessage(message, peer.Key).Wait();
            }

            Console.WriteLine(message);
        }

        private async Task SendLoop()
        {
            while (state == State.WaitingForParentConnection)
            {
                byte[] message = {(byte) CommandCode.ConnectToParent};
                await udpclient.SendAsync(message, message.Length, parentEndPoint).ConfigureAwait(false);

                await Task.Delay(1000).ConfigureAwait(false);
            }

            while (true)
            {
                foreach (var peer in peers)
                {
                    foreach (var message in peer.Value.PendingMessagesLastSendAttempt)
                    {
                        if (DateTime.Now - message.Value.LastSendTime > retryTimeout)
                        {
                            await this.SendMessage(message.Key, peer.Key).ConfigureAwait(false);

                            var newSentData = new MessageSentData(DateTime.Now, message.Value.SendAttempts + 1);
                            peer.Value.PendingMessagesLastSendAttempt[message.Key] = newSentData;
                        }
                    }
                }

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        private async Task ReceiveLoop()
        {
            while (true)
            {
                UdpReceiveResult packet = await udpclient.ReceiveAsync().ConfigureAwait(false);

                if (new Random().Next(100) < lossPercent)
                {
                    Console.WriteLine($"Whoops! Packet from {packet.RemoteEndPoint} dropped");
                    continue;
                }

                try
                {
                    await RecievePacket(packet).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error when handling packet from {packet.RemoteEndPoint}: {e.Message}");
                    throw;
                }
            }
        }

        private async Task RecievePacket(UdpReceiveResult packet)
        {
            switch ((CommandCode) packet.Buffer[0])
            {
                case CommandCode.ConnectToParent:
                    await this.OnChildConnected(packet).ConfigureAwait(false);
                    break;

                case CommandCode.ConnectToParentAck:
                    this.OnConnectedToParent(packet);
                    break;

                case CommandCode.Message:
                    await this.MessageRecieved(packet).ConfigureAwait(false);
                    break;

                case CommandCode.MessageAck:
                    byte[] guidBytes = new byte[packet.Buffer.Length - 1];
                    Array.Copy(packet.Buffer, 1, guidBytes, 0, guidBytes.Length);

                    var messageToRemove = new Message(new Guid(guidBytes));
                    peers[packet.RemoteEndPoint].PendingMessagesLastSendAttempt.TryRemove(messageToRemove, out _);
                    break;

                case CommandCode.Dead:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task OnChildConnected(UdpReceiveResult packet)
        {
            peers.TryAdd(packet.RemoteEndPoint, new Peer());

            byte[] message;
            if (parentEndPoint != null)
            {
                byte[] addressBytes = parentEndPoint.Address.GetAddressBytes();
                byte[] portBytes = BitConverter.GetBytes(parentEndPoint.Port);

                message = new byte[1 + addressBytes.Length + portBytes.Length];
                message[0] = (byte) CommandCode.ConnectToParentAck;
                Array.Copy(addressBytes, 0, message, 1, addressBytes.Length);
                Array.Copy(portBytes, 0, message, 1 + addressBytes.Length, portBytes.Length);
            }
            else
            {
                message = new[] {(byte) CommandCode.ConnectToParentAck};
            }

            await udpclient.SendAsync(message, message.Length, packet.RemoteEndPoint).ConfigureAwait(false);

            Console.WriteLine($"[INFO] New child connected: {packet.RemoteEndPoint}");
        }

        private void OnConnectedToParent(UdpReceiveResult packet)
        {
            if (!packet.RemoteEndPoint.Equals(parentEndPoint))
            {
                Console.WriteLine($"[WARN] Got ConnectToParentAck from {packet.RemoteEndPoint}, ignoring");
            }

            if (state != State.WaitingForParentConnection)
            {
                return;
            }

            if (packet.Buffer.Length == 1)
            {
                this.parentsParentEndPoint = null;

                state = State.Working;
                Console.WriteLine("[INFO] Connected to parent. Parent is root");
            }
            else
            {
                byte[] addressBytes = new byte[packet.Buffer.Length - 5];
                byte[] portBytes = new byte[4];

                Array.Copy(packet.Buffer, 1, addressBytes, 0, addressBytes.Length);
                Array.Copy(packet.Buffer, packet.Buffer.Length - 4, portBytes, 0, 4);

                IPAddress address = new IPAddress(addressBytes);
                int port = BitConverter.ToInt32(portBytes, 0);

                this.parentsParentEndPoint = new IPEndPoint(address, port);

                state = State.Working;
                Console.WriteLine("[INFO] Connected to parent. Parent's parent ip: " + this.parentsParentEndPoint);
            }
        }

        private async Task SendMessage(Message message, IPEndPoint recipient)
        {
            byte[] messageSerialized;
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, message);
                messageSerialized = ms.GetBuffer();
            }

            byte[] buffer = new byte[messageSerialized.Length + 1];
            buffer[0] = (byte) CommandCode.Message;
            Array.Copy(messageSerialized, 0, buffer, 1, messageSerialized.Length);

            await udpclient.SendAsync(buffer, buffer.Length, recipient).ConfigureAwait(false);
        }

        private async Task MessageRecieved(UdpReceiveResult data)
        {
            Message message;
            using (var ms = new MemoryStream(data.Buffer, 1, data.Buffer.Length - 1))
            {
                message = (Message) new BinaryFormatter().Deserialize(ms);
            }

            byte[] guidSerialized = message.Id.ToByteArray();
            byte[] buffer = new byte[guidSerialized.Length + 1];
            buffer[0] = (byte) CommandCode.MessageAck;
            Array.Copy(guidSerialized, 0, buffer, 1, guidSerialized.Length);

            await udpclient.SendAsync(buffer, buffer.Length, data.RemoteEndPoint).ConfigureAwait(false);

            if (lastRecievedMessages.TryAdd(message.Id, message))
            {
                Console.WriteLine(message);

                foreach (var peer in peers)
                {
                    if (!peer.Key.Equals(data.RemoteEndPoint))
                    {
                        peer.Value.PendingMessagesLastSendAttempt.TryAdd(message, new MessageSentData(DateTime.Now));
                        await this.SendMessage(message, peer.Key).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
