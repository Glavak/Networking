using System;
using System.Text;
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

                byte[] recievedData = new byte[20];
                socket.Recieve(recievedData, 20);
                string recievedDataStr = Encoding.UTF8.GetString(recievedData);
                Console.WriteLine($"Recieved data: {recievedDataStr}");
            }
        }
    }
}
