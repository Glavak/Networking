using System;
using System.Text;
using System.Threading;
using ATP;

namespace Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (AtpListenSocket listener = new AtpListenSocket(4242))
            {
                var socket = listener.Accept();

                var dataString = "Some SERVER test data, to send through socket";
                byte[] data = Encoding.UTF8.GetBytes(dataString);
                socket.Send(data, 0, data.Length);
                Console.WriteLine($"Sent data: {dataString}");

                byte[] recievedData = new byte[100];
                int count = socket.Recieve(recievedData, 100);
                string recievedDataStr = Encoding.UTF8.GetString(recievedData, 0, count);
                Console.WriteLine($"Recieved data ({count} bytes): {recievedDataStr}");

                Thread.Sleep(1000);
            }
        }
    }
}
