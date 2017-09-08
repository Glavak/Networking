using System;
using System.Net;

namespace TreeChat
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            TreeChat chat;

            if (args.Length == 2)
            {
                chat = new TreeChat(args[0], 0, null, int.Parse(args[1]));
            }
            else if (args.Length == 4)
            {
                chat = new TreeChat(args[0], 0, new IPEndPoint(IPAddress.Parse(args[2]), int.Parse(args[3])),
                    int.Parse(args[1]));
            }
            else
            {
                return;
            }

            chat.Start();

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
