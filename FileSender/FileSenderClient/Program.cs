using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FileSenderClient
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Specify parameters");
                return;
            }

            var fileInfo = new FileInfo(args[0]);
            var reader = new FileStream(args[0], FileMode.Open, FileAccess.Read);

            byte[] buff;
            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(IPAddress.Parse(args[1]), int.Parse(args[2]));

                byte[] filenameBuffer = Encoding.UTF8.GetBytes(fileInfo.Name);
                socket.Send(filenameBuffer);
                socket.Send(new byte[] {0});
                socket.Send(BitConverter.GetBytes(fileInfo.Length));

                buff = new byte[1024];
                while (true)
                {
                    int read = reader.Read(buff, 0, 1024);
                    if (read == 0)
                    {
                        break;
                    }
                    socket.Send(buff, 0, read, SocketFlags.None);
                }

                int readSuccessState = socket.Receive(buff, 1, SocketFlags.None);
                if (readSuccessState == 1 && buff[0] == 42)
                {
                    Console.WriteLine("File successfully sent");
                }
                else
                {
                    Console.WriteLine("Error sending file");
                }
            }
        }
    }
}
