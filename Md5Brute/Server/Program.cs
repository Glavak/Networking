using System.Security.Cryptography;
using System.Text;

namespace Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string hashString = args[0];
            int port = int.Parse(args[1]);

            byte[] bytes = Encoding.UTF8.GetBytes("RACACACAGTCA");
            var hash = new MD5Cng().ComputeHash(bytes);

//            var hash = StringToByteArrayFastest(hashString);

            new Server(hash).Start(port);
        }

        public static byte[] StringToByteArrayFastest(string hex)
        {
            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte) ((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = hex;

            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
