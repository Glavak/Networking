using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileSenderServer
{
    internal class Program
    {
        private const int Port = 54001;
        private const int BufferSize = 4096 * 2;

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
                    string filepath = null;
                    byte[] buff = new byte[BufferSize];
                    IPEndPoint remoteEndPoint = (IPEndPoint) ((Socket) sender).RemoteEndPoint;
                    DateTime started = DateTime.Now;
                    DateTime lastRecieve = DateTime.Now;
                    try
                    {
                        int read = 0;
                        int zeroPosition = -1;
                        while (zeroPosition == -1 || read - zeroPosition < 5)
                        {
                            read += await stream.ReadAsync(buff, read, BufferSize - read);
                            zeroPosition = buff.ToList().IndexOf(0);
                        }

                        string filename = Encoding.UTF8.GetString(buff, 0, zeroPosition);
                        long filesize = BitConverter.ToInt64(buff, zeroPosition + 1);

                        Console.WriteLine("1");

                        Directory.CreateDirectory($"uploads/from {remoteEndPoint.Address}");
                        filepath = $"uploads/from {remoteEndPoint.Address}/{filename}";

                        if (filename.Contains("/") || filename.Contains("\\"))
                        {
                            Console.WriteLine($"Incorrect file name: {filename}");
                            return;
                        }

                        Console.WriteLine("2");

                        long totalRead;
                        using (FileStream writer = new FileStream(filepath, FileMode.Open, FileAccess.Write))
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
                            $"Accepted file {filename} of size {(float)filesize/1000:.0}KB from client {remoteEndPoint.Address}");
                        Console.WriteLine(
                            $"Avg speed during transmission: {totalRead / (DateTime.Now - started).TotalMilliseconds:.0}KB/s");
                        ((Socket) sender).Send(new byte[] {42}, 1, SocketFlags.None);
                    }
                    catch (Exception e)
                    {
                        if(filepath != null)
                        File.Delete(filepath);
                        Console.WriteLine($"Error from client {remoteEndPoint.Address}: {e.Message}");
                    }
                    finally
                    {
                        ((Socket) sender).Close();
                    }
                };

                Task serverTask = server.StartAsync();
                Console.WriteLine("Server started, listening to incoming connections");
                serverTask.Wait();
            }
        }
    }
}
