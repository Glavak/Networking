using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ATP
{
    public class AtpClientSocket : AtpSocket
    {
        private bool disposed = false;

        private readonly UdpClient udpclient;
        private readonly IPEndPoint server;

        private bool stopping;

        private ClientSocketState state;

        /// <summary>
        /// Creates AtpSocket object and connects to specified server
        /// </summary>
        /// <param name="server">Endpoint of server, accepting ATP connections</param>
        public AtpClientSocket(IPEndPoint server)
        {
            this.udpclient = new UdpClient();
            this.server = server;

            var sendTask = Task.Run(async () => await SendLoop());
            var recieveTask = Task.Run(async () => await ReceiveLoop());

            // Wait for connection to be established
            lock (this)
            {
                while (state == ClientSocketState.Connecting)
                {
                    byte[] message = {(byte) CommandCode.Connect};
                    udpclient.Send(message, 1, server);

                    Monitor.Wait(this, TimeSpan.FromSeconds(2));
                }
            }
        }

        private async Task SendLoop()
        {
            while (!stopping)
            {
                byte[] message;

                lock (SendBuffer)
                {
                    int bytesAvailible = SendBuffer.GetAvailibleBytesAtBegin();
                    if (bytesAvailible <= 0)
                    {
                        Monitor.Wait(SendBuffer);
                        continue;
                    }

                    int toRead = bytesAvailible < SendBy ? bytesAvailible : SendBy;

                    message = new byte[1 + 8 + toRead];

                    message[0] = (byte) CommandCode.Data;

                    byte[] lengthBuffer = BitConverter.GetBytes(SendBuffer.FirstAvailibleAbsolutePosition);
                    Array.Copy(lengthBuffer, 0, message, 1, lengthBuffer.Length);

                    byte[] buffer = new byte[toRead];
                    SendBuffer.TryGetFromBegin(buffer, toRead);
                    Monitor.PulseAll(SendBuffer);
                    Array.Copy(buffer, 0, message, 1 + 8, toRead);
                }

                //Console.WriteLine($"Sending {message.Length} bytes of message");
                await udpclient.SendAsync(message, message.Length, server);
            }
        }

        private async Task ReceiveLoop()
        {
            while (!stopping)
            {
                UdpReceiveResult packet;
                try
                {
                    packet = await udpclient.ReceiveAsync().ConfigureAwait(false);
                }
                catch (SocketException)
                {
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
            long startAbsolutePosition;
            switch ((CommandCode) packet.Buffer[0])
            {
                case CommandCode.ConnectAck:
                    lock (this)
                    {
                        state = ClientSocketState.Working;
                        Monitor.Pulse(this);
                    }

                    Console.WriteLine($"Connect acked");

                    break;

                case CommandCode.Data:
                    Console.WriteLine($"Data !");
                    startAbsolutePosition = BitConverter.ToInt64(packet.Buffer, 1);

                    bool added;
                    lock (RecieveBuffer)
                    {
                        try
                        {
                            added = RecieveBuffer.TryAdd(packet.Buffer, 9, packet.Buffer.Length - 9,
                                startAbsolutePosition);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            break;
                        }
                        Monitor.PulseAll(RecieveBuffer);
                    }

                    Console.WriteLine($"Data got, added:{added}");

                    if (added)
                    {
                        byte[] message = new byte[1 + 8 + 8];
                        message[0] = (byte) CommandCode.DataAck;
                        Array.Copy(packet.Buffer, 1, message, 1, 8);

                        byte[] length = BitConverter.GetBytes((long) (packet.Buffer.Length - 9));
                        Array.Copy(length, 0, message, 1 + 8, 8);

                        await udpclient.SendAsync(message, message.Length, packet.RemoteEndPoint);
                    }
                    else
                    {
                        byte[] message = {(byte) CommandCode.Overloaded};
                        await udpclient.SendAsync(message, message.Length, packet.RemoteEndPoint);
                    }

                    break;

                case CommandCode.DataAck:
                    startAbsolutePosition = BitConverter.ToInt64(packet.Buffer, 1);
                    long dataBytesCount = BitConverter.ToInt64(packet.Buffer, 1 + 8);

                    Console.WriteLine($"Data ack got pos: {startAbsolutePosition} count: {dataBytesCount}");

                    lock (SendBuffer)
                    {
                        SendBuffer.DisposeElements(startAbsolutePosition, (int) dataBytesCount);
                        Monitor.PulseAll(SendBuffer);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                udpclient.Close();
            }

            disposed = true;
        }
    }
}
