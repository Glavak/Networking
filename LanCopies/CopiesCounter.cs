using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LanCopies
{
    public class CopiesCounter
    {
        private const int Port = 4242;
        private const byte PingCommand = 42;
        private const byte SessionEndCommand = 43;

        private bool terminate;

        private readonly IPAddress multicastAddress;

        private readonly ConcurrentDictionary<IPEndPoint, DateTime> aliveAddresses =
            new ConcurrentDictionary<IPEndPoint, DateTime>();

        public CopiesCounter(IPAddress multicastAddress)
        {
            this.multicastAddress = multicastAddress;
        }

        public void Start()
        {
            new Thread(SendPingLoop).Start();
            new Thread(ReceiveLoop).Start();
            new Thread(PrintLoop).Start();
        }

        public void Stop()
        {
            terminate = true;
        }

        private void SendPingLoop()
        {
            UdpClient udpclient = new UdpClient(multicastAddress.AddressFamily);
            IPEndPoint remoteep = new IPEndPoint(multicastAddress, Port);
            udpclient.JoinMulticastGroup(remoteep.Address);

            byte[] buffer = {PingCommand};
            while (!terminate)
            {
                udpclient.Send(buffer, buffer.Length, remoteep);
                Thread.Sleep(1000);
            }

            buffer[0] = SessionEndCommand;
            udpclient.Send(buffer, buffer.Length, remoteep);
        }

        private void ReceiveLoop()
        {
            UdpClient udpclient = new UdpClient(multicastAddress.AddressFamily);
            udpclient.ExclusiveAddressUse = false;

            IPAddress any = multicastAddress.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any;
            IPEndPoint localEp = new IPEndPoint(any, Port);

            udpclient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            udpclient.Client.Bind(localEp);

            udpclient.JoinMulticastGroup(multicastAddress);

            while (!terminate)
            {
                byte[] data = udpclient.Receive(ref localEp);
                switch (data[0])
                {
                    case PingCommand:
                        if (!aliveAddresses.TryAdd(localEp, DateTime.Now))
                        {
                            aliveAddresses[localEp] = DateTime.Now;
                        }
                        break;
                    case SessionEndCommand:
                        DateTime t;
                        aliveAddresses.TryRemove(localEp, out t);
                        break;
                }
            }
        }

        private void PrintLoop()
        {
            bool dash = false;
            while (!terminate)
            {
                dash = !dash;
                Console.Clear();
                Console.WriteLine((dash ? "-" : "|") + " Press Q to quit");
                Console.WriteLine();

                foreach (var aliveAddress in aliveAddresses)
                {
                    if (aliveAddress.Value.AddSeconds(3) > DateTime.Now)
                    {
                        Console.WriteLine(aliveAddress.Key);
                    }
                    else
                    {
                        DateTime t;
                        aliveAddresses.TryRemove(aliveAddress.Key, out t);
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
