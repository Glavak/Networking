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

        private readonly Task sendTask;
        private readonly Task recieveTask;

        private readonly TimeSpan resendTimeout = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan disconnectTimeout = TimeSpan.FromMilliseconds(1500);
        private readonly TimeSpan overloadedPause = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Creates AtpSocket object and connects to specified server
        /// </summary>
        /// <param name="server">Endpoint of server, accepting ATP connections</param>
        public AtpClientSocket(IPEndPoint server)
        {
            this.udpclient = new UdpClient();
            this.server = server;

            sendTask = Task.Run(SendLoop);

            // Wait for connection to be established
            lock (this)
            {
                while (state == ClientSocketState.Connecting)
                {
                    byte[] message = {(byte) CommandCode.Connect};
                    udpclient.Send(message, 1, server);

                    if(recieveTask == null) recieveTask = Task.Run(ReceiveLoop);

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
                    if (DateTime.Now - LastSend < resendTimeout)
                    {
                        Monitor.Wait(SendBuffer, resendTimeout - (DateTime.Now - LastSend));
                        continue;
                    }

                    if (failedSendAttempts > 5)
                    {
                        dead = true;
                        return;
                    }

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

                Console.WriteLine($"Sending {message.Length} bytes to server");
                try
                {
                    await udpclient.SendAsync(message, message.Length, server);
                    LastSend = DateTime.Now;
                    failedSendAttempts++;
                }
                catch (SocketException)
                {
                }
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
                catch (ObjectDisposedException)
                {
                    return;
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

                    LastRecieved = DateTime.Now;
                    Console.WriteLine($"Connect acked");

                    break;

                case CommandCode.Data:
                    Console.WriteLine($"Incoming data");
                    startAbsolutePosition = BitConverter.ToInt64(packet.Buffer, 1);

                    LastRecieved = DateTime.Now;

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

                    LastRecieved = DateTime.Now;
                    LastSend = DateTime.Now + overloadedPause; // For next data to be sent w/o timeout
                    failedSendAttempts = 0;

                    Console.WriteLine($"Data ack got pos: {startAbsolutePosition} count: {dataBytesCount}");

                    lock (SendBuffer)
                    {
                        SendBuffer.DisposeElements(startAbsolutePosition, (int) dataBytesCount);
                        Monitor.PulseAll(SendBuffer);
                    }

                    break;

                case CommandCode.Overloaded:
                    LastRecieved = DateTime.Now;
                    LastSend = DateTime.Now + overloadedPause; // For next data to be sent after pause
                    failedSendAttempts = 0;

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
                lock (SendBuffer)
                {
                    while (SendBuffer.GetAvailibleBytesAtBegin() > 0 && DateTime.Now-LastRecieved < disconnectTimeout)
                    {
                        Monitor.Wait(SendBuffer, resendTimeout);
                    }
                }

                stopping = true;
                udpclient.Close();

                Task.WaitAll(sendTask, recieveTask);
            }

            disposed = true;
        }
    }
}
