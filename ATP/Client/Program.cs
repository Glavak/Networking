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
                Console.WriteLine($"! Sent data: {dataString}");

                byte[] recievedData = new byte[100];
                int count = socket.Recieve(recievedData, 100);
                string recievedDataStr = Encoding.UTF8.GetString(recievedData, 0, count);
                Console.WriteLine($"! Recieved data ({count} bytes): {recievedDataStr}");
            }

            Console.WriteLine("! Connection closed");
        }
    }
}
