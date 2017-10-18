using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class TcpServer : IDisposable
    {
        private readonly TcpListener listener;
        private CancellationTokenSource tokenSource;

        public event EventHandler<NetworkStream> OnDataReceived;

        public TcpServer(IPAddress address, int port)
        {
            listener = new TcpListener(address, port);
        }

        public bool Listening { get; private set; }

        public async Task StartAsync()
        {
            tokenSource = new CancellationTokenSource();
            tokenSource.Token.Register(() => listener.Stop());
            listener.Start();
            Listening = true;

            try
            {
                while (!tokenSource.Token.IsCancellationRequested)
                {
                    await Task.Run(async () =>
                    {
                        var result = await listener.AcceptTcpClientAsync();
                        OnDataReceived?.Invoke(result.Client, result.GetStream());
                    }, tokenSource.Token);
                }
            }
            catch (ObjectDisposedException)
            {
                // If we cancelled the token and that Stopped listener, AcceptTcpClientAsync() will throw this exception
            }
            finally
            {
                listener.Stop();
                Listening = false;
            }
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
