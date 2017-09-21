using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace FileSenderServer
{
    internal class Program
    {
        private const int Port = 54001;
        private const int BufferSize = 4096;

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Specify port, for waiting for incoming connections");
                return;
            }

            using (var server = new TcpServer(IPAddress.Any, Port))
            {
                server.OnDataReceived += async (sender, stream) =>
                {
                    byte[] buff = new byte[BufferSize];
                    IPEndPoint remoteEndPoint = (IPEndPoint) ((Socket) sender).RemoteEndPoint;
                    DateTime started = DateTime.Now;
                    DateTime lastRecieve = DateTime.Now;
                    try
                    {
                        int read = await stream.ReadAsync(buff, 0, BufferSize);
                        int zeroPosition = buff.ToList().IndexOf(0);

                        string filename = Encoding.UTF8.GetString(buff, 0, zeroPosition);
                        long filesize = BitConverter.ToInt64(buff, zeroPosition + 1);

                        Directory.CreateDirectory($"uploads/from {remoteEndPoint.Address}");
                        string filepath = $"uploads/from {remoteEndPoint.Address}/{filename}";

                        if (filename.Contains("/") || filename.Contains("\\"))
                        {
                            Console.WriteLine($"Incorrect file name: {filename}");
                            return;
                        }

                        long totalRead;
                        using (FileStream writer = new FileStream(filepath, FileMode.Create, FileAccess.Write))
                        {
                            await writer.WriteAsync(buff, zeroPosition + 9, read - zeroPosition - 9);
                            totalRead = read - zeroPosition - 9;
                            long lastRead = read - zeroPosition - 9;

                            while (filesize > totalRead)
                            {
                                read = await stream.ReadAsync(buff, 0, BufferSize);
                                if (read == 0)
                                {
                                    break;
                                }
                                await writer.WriteAsync(buff, 0, read);
                                totalRead += read;
                                lastRead += read;

                                //Console.SetCursorPosition(0,0);
                                //Console.WriteLine((DateTime.Now - lastRecieve).Milliseconds);
                                if (DateTime.Now > lastRecieve.AddSeconds(1))
                                {
                                    Console.WriteLine(
                                        $"Current speed: {lastRead / (DateTime.Now - lastRecieve).TotalMilliseconds:.0}KB/s");
                                    Console.WriteLine(
                                        $"Avg speed: {totalRead / (DateTime.Now - started).TotalMilliseconds:.0}KB/s");

                                    lastRead = 0;
                                    lastRecieve = DateTime.Now;
                                }
                            }
                        }
                        Console.WriteLine(
                            $"Accepted file {filename} of size {filesize}B from client {remoteEndPoint.Address}");
                        Console.WriteLine(
                            $"Avg speed: {totalRead / (DateTime.Now - started).TotalMilliseconds:.0}KB/s");
                        ((Socket) sender).Send(new byte[] {42}, 1, SocketFlags.None);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error from client {remoteEndPoint.Address}: {e.Message}");
                    }
                    finally
                    {
                        ((Socket) sender).Close();
                    }
                };

                server.StartAsync().Wait();
            }
        }
    }
}
