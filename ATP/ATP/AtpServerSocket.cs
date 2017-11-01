using System.Net;
using System.Net.Sockets;

namespace ATP
{
    public class AtpServerSocket : AtpSocket
    {
        private bool disposed = false;

        private readonly UdpClient udpclient;
        private readonly IPEndPoint client;

        internal AtpServerSocket(UdpClient udpclient, IPEndPoint client)
        {
            this.udpclient = udpclient;
            this.client = client;
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
