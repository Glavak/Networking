using System;
using System.Net;
using System.Text;
using System.Threading;
using ATP;

namespace Client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (AtpClientSocket socket = new AtpClientSocket(new IPEndPoint(IPAddress.Loopback, 4242)))
            {
                var dataString = "Some test data, to send through socket";
                byte[] data = Encoding.UTF8.GetBytes(dataString);
                socket.Send(data, 0, data.Length);
                Console.WriteLine($"Sent data: {dataString}");

                byte[] recievedData = new byte[20];
                socket.Recieve(recievedData, 20);
                string recievedDataStr = Encoding.UTF8.GetString(recievedData);
                Console.WriteLine($"Recieved data: {recievedDataStr}");

                Thread.Sleep(1000);
            }
        }
    }
}
