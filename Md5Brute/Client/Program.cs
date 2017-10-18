using System;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            IPAddress address = IPAddress.Parse(args[0]);
            int port = int.Parse(args[1]);

            var client = new Client();
            while (!client.IsFinished)
            {
                TryDo(() => client.GetJob(address, port), 5);

                if (client.IsFinished) return;

                client.FindHash();

                TryDo(() => client.SubmitResult(address, port), 5);
            }
        }

        private static void TryDo(Action action, int attempts)
        {
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (SocketException)
                {
                }

                if (attempts-- <= 0)
                {
                    Console.WriteLine("Server unavailible");
                    Environment.Exit(1);
                }
                Console.WriteLine("Server unavailible trying again after 3s");
                Thread.Sleep(3000);
            }
        }
    }
}
