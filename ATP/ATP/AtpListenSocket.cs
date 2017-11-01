using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ATP
{
    public class AtpListenSocket : IDisposable
    {
        private bool disposed = false;

        private readonly UdpClient udpclient;
        private readonly Queue<IPEndPoint> acceptQueue;
        private readonly ConcurrentDictionary<IPEndPoint, AtpServerSocket> clients;

        private bool stopping;

        /// <summary>
        /// Creates AtpListenSocket object and starts listening for incoming ATP connections on specified port
        /// </summary>
        /// <param name="port"></param>
        public AtpListenSocket(int port)
        {
            udpclient = new UdpClient(port);
            acceptQueue = new Queue<IPEndPoint>();
            clients = new ConcurrentDictionary<IPEndPoint, AtpServerSocket>();

            Task.Run(SendLoop);
            Task.Run(ReceiveLoop);
        }

        public AtpSocket Accept()
        {
            lock (acceptQueue)
            {
                while (acceptQueue.Count == 0)
                {
                    Monitor.Wait(acceptQueue);
                }

                IPEndPoint sender = acceptQueue.Dequeue();
                var socket = new AtpServerSocket(udpclient, sender);
                clients.TryAdd(sender, socket);
                return socket;
            }
        }

        private async Task SendLoop()
        {
            while (!stopping)
            {
                foreach (var atpServerSocket in clients)
                {
                    byte[] message;

                    lock (atpServerSocket.Value.SendBuffer)
                    {
                        int bytesAvailible = atpServerSocket.Value.SendBuffer.GetAvailibleBytesAtBegin();
                        if (bytesAvailible <= 0) continue;

                        int toRead = atpServerSocket.Value.SendBy;
                        if (toRead > bytesAvailible) toRead = bytesAvailible;

                        message = new byte[1 + 8 + toRead];

                        message[0] = (byte) CommandCode.Data;

                        byte[] lengthBuffer = BitConverter.GetBytes(atpServerSocket.Value.SendBuffer.FirstAvailibleAbsolutePosition);
                        Array.Copy(lengthBuffer, 0, message, 1, lengthBuffer.Length);

                        byte[] buffer = new byte[toRead];
                        atpServerSocket.Value.SendBuffer.TryGetFromBegin(buffer, toRead);
                        Monitor.PulseAll(atpServerSocket.Value.SendBuffer);
                        Array.Copy(buffer, 0, message, 1 + 8, toRead);
                    }

                    //Console.WriteLine($"Sending {message.Length} bytes of message to {atpServerSocket.Key}");
                    await udpclient.SendAsync(message, message.Length, atpServerSocket.Key);
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
            byte[] message;
            long startAbsolutePosition;
            AtpServerSocket atpServerSocket;

            switch ((CommandCode) packet.Buffer[0])
            {
                case CommandCode.Connect:
                    if (!clients.ContainsKey(packet.RemoteEndPoint))
                    {
                        lock (acceptQueue)
                        {
                            acceptQueue.Enqueue(packet.RemoteEndPoint);
                            Monitor.Pulse(acceptQueue);
                        }
                    }

                    message = new[] {(byte) CommandCode.ConnectAck};
                    await udpclient.SendAsync(message, message.Length, packet.RemoteEndPoint);

                    Console.WriteLine($"Connect got from {packet.RemoteEndPoint}");

                    break;

                case CommandCode.Data:
                    Console.WriteLine($"Data !");
                    startAbsolutePosition = BitConverter.ToInt64(packet.Buffer, 1);

                    atpServerSocket = clients[packet.RemoteEndPoint];

                    bool added;
                    lock (atpServerSocket.RecieveBuffer)
                    {
                        added = atpServerSocket.RecieveBuffer.TryAdd(packet.Buffer, 9, packet.Buffer.Length - 9,
                            startAbsolutePosition);
                        Monitor.PulseAll(atpServerSocket.RecieveBuffer);
                    }

                    Console.WriteLine($"Data got from {packet.RemoteEndPoint}, added: {added}");

                    if (added)
                    {
                        message = new byte[1+8+8];
                        message[0] = (byte) CommandCode.DataAck;
                        Array.Copy(packet.Buffer, 1, message, 1, 8);

                        byte[] length = BitConverter.GetBytes((long) (packet.Buffer.Length - 9));
                        Array.Copy(length, 0, message, 1+8, 8);

                        await udpclient.SendAsync(message, message.Length, packet.RemoteEndPoint);
                    }
                    else
                    {
                        message = new[] {(byte) CommandCode.Overloaded};
                        await udpclient.SendAsync(message, message.Length, packet.RemoteEndPoint);
                    }

                    break;

                case CommandCode.DataAck:
                    startAbsolutePosition = BitConverter.ToInt64(packet.Buffer, 1);
                    long dataBytesCount = BitConverter.ToInt64(packet.Buffer, 1+8);

                    atpServerSocket = clients[packet.RemoteEndPoint];

                    Console.WriteLine($"Data ack got from {packet.RemoteEndPoint}  pos: {startAbsolutePosition} count: {dataBytesCount}");

                    lock (atpServerSocket.SendBuffer)
                    {
                        atpServerSocket.SendBuffer.DisposeElements(startAbsolutePosition, (int) dataBytesCount);
                        Monitor.PulseAll(atpServerSocket.SendBuffer);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // TODO: close sockets
            }

            disposed = true;
        }
    }
}
