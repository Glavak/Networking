using System;
using System.Net;

namespace TreeChat
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            IPEndPoint endPoint;

            if (args.Length == 2)
            {
                endPoint = null;
            }
            else if (args.Length == 4)
            {
                endPoint = new IPEndPoint(IPAddress.Parse(args[2]), int.Parse(args[3]));
            }
            else
            {
                Console.WriteLine("Invalid arguments");
                return;
            }

            TreeChat chat = new TreeChat(args[0], 10, endPoint, int.Parse(args[1]));
            chat.Start();

            AppDomain.CurrentDomain.DomainUnload += (sender, eventArgs) => chat.BroadcastDead().Wait();

            while (true)
            {
                string messageText = Console.ReadLine();
                if (messageText?.ToLower() == "quit")
                {
                    chat.Stop();
                    return;
                }

                chat.SendMessage(messageText);
            }
        }
    }
}
