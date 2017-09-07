using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FileSenderServer
{
    public class TcpServer : IDisposable
    {
        private readonly TcpListener listener;
        private CancellationTokenSource tokenSource;
        private CancellationToken token;

        public event EventHandler<NetworkStream> OnDataReceived;

        public TcpServer(IPAddress address, int port)
        {
            listener = new TcpListener(address, port);
        }

        public bool Listening { get; private set; }

        public async Task StartAsync()
        {
            tokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken());
            this.token = tokenSource.Token;
            listener.Start();
            Listening = true;

            try
            {
                while (!this.token.IsCancellationRequested)
                {
                    await Task.Run(async () =>
                    {
                        var tcpClientTask = listener.AcceptTcpClientAsync();
                        var result = await tcpClientTask;
                        OnDataReceived?.Invoke(result.Client, result.GetStream());
                    }, this.token);
                }
            }
            finally
            {
                listener.Stop();
                Listening = false;
            }
        }

        public void Stop()
        {
            tokenSource?.Cancel();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
