using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ATP
{
    public class AtpServerSocket : AtpSocket
    {
        private bool disposed = false;

        private readonly UdpClient udpclient;
        private readonly IPEndPoint client;

        private readonly AtpListenSocket parent;

        internal AtpServerSocket(UdpClient udpclient, IPEndPoint client, AtpListenSocket parent)
        {
            this.udpclient = udpclient;
            this.client = client;
            this.parent = parent;
        }

        public override void Send(byte[] buff, int offset, int count)
        {
            base.Send(buff, offset, count);

            lock (parent)
            {
                Monitor.PulseAll(parent);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
            }

            disposed = true;
        }
    }
}
