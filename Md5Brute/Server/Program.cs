using System.Security.Cryptography;
using System.Text;

namespace Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            int port = int.Parse(args[0]);

            byte[] bytes = Encoding.UTF8.GetBytes("AACACACAGTCA");
            var hash = new MD5Cng().ComputeHash(bytes);
            new Server(hash).Start(port);
        }
    }
}
