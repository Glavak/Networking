using System.Net;

namespace Client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var client = new Client();
            while (!client.IsFinished)
            {
                client.GetJob(IPAddress.Loopback, 4242);
                client.FindHash();
                client.SubmitResult(IPAddress.Loopback, 4242);
            }
        }
    }
}
