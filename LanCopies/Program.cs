using System;
using System.Net;

namespace LanCopies
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("You should specify multicast address");
                return;
            }

            var copiesCounter = new CopiesCounter(IPAddress.Parse(args[0]));

            copiesCounter.Start();

            ConsoleKey k;
            do
            {
                k = Console.ReadKey(true).Key;
            } while (k != ConsoleKey.Q);

            copiesCounter.Stop();
        }
    }
}
