using System.Security.Cryptography;
using System.Text;

namespace Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("AAC");
            var hash = new MD5Cng().ComputeHash(bytes);
            new Server(hash).Start();
        }
    }
}
