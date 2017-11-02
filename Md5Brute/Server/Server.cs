using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Model;

namespace Server
{
    public class Server
    {
        private const int MaxStringLength = 12;
        private const int PerClientLength = 10;

        private byte[] desiredHash;

        private bool finished;

        private readonly Dictionary<string, AssociatedClient> prefixes = new Dictionary<string, AssociatedClient>();
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(10);

        public Server(byte[] desiredHash)
        {
            this.desiredHash = desiredHash;

            for (int len = 0; len <= MaxStringLength - PerClientLength; len++)
            {
                char[] prefix = Transmutations.First(len);

                do
                {
                    prefixes.Add(new string(prefix), null);
                } while (Transmutations.Next(prefix));
            }
        }

        public void Start(int port)
        {
            using (var server = new TcpServer(IPAddress.Any, port))
            {
                server.OnDataReceived += async (sender, stream) =>
                {
                    BinaryReader reader = new BinaryReader(stream);
                    RequestOpcode request = (RequestOpcode) reader.ReadByte();
                    byte[] guidBytes;
                    Guid id;

                    switch (request)
                    {
                        case RequestOpcode.JobRequest:
                            guidBytes = reader.ReadBytes(16);
                            id = new Guid(guidBytes);

                            BinaryWriter writer = new BinaryWriter(stream);

                            if (finished ||
                                prefixes.All(x => x.Value != null &&
                                                  DateTime.Now - x.Value.LastActivity < timeout))
                            {
                                Console.WriteLine($"No more jobs for {id}");
                                writer.Write(0);
                                return;
                            }

                            var prefix = prefixes.First(x => x.Value == null ||
                                                             DateTime.Now - x.Value.LastActivity >= timeout).Key;
                            prefixes[prefix] = new AssociatedClient(id, DateTime.Now);

                            if (prefix == "")
                            {
                                writer.Write(-PerClientLength);
                            }
                            else
                            {
                                writer.Write(PerClientLength);
                            }

                            writer.Write(prefix);
                            writer.Write(desiredHash.Length);
                            writer.Write(desiredHash);

                            Console.WriteLine($"Client {id} connected, given job \"{prefix}\"");

                            break;

                        case RequestOpcode.HashNotFound:
                            guidBytes = reader.ReadBytes(16);
                            id = new Guid(guidBytes);

                            Console.WriteLine($"Client {id} finished, hash not found");

                            if (prefixes.Any(p => p.Value != null && p.Value.guid == id))
                            {
                                var tmp = prefixes.FirstOrDefault(p => p.Value.guid == id);
                                string hisPrefix = tmp.Key;
                                prefixes.Remove(hisPrefix);

                                if (prefixes.Count == 0)
                                {
                                    Console.WriteLine("All prefixes checked, no result found");
                                    finished = true;
                                    await Task.Delay(timeout);
                                    server.Stop();
                                }
                            }
                            break;

                        case RequestOpcode.HashFound:
                            string result = reader.ReadString();
                            Console.WriteLine($"Result found, string: {result}");
                            Console.WriteLine("Waiting for all the clients to finish");
                            finished = true;

                            await Task.Delay(timeout);
                            while (prefixes.Any(x => x.Value != null &&
                                                     DateTime.Now - x.Value.LastActivity < timeout))
                            {
                                await Task.Delay(1000);
                                Console.WriteLine("Waiting..");
                            }

                            server.Stop();
                            Console.WriteLine("Stopping..");
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                };

                Task serverTask = server.StartAsync();
                Console.WriteLine("Server started, listening to incoming connections");
                serverTask.Wait();
                Console.WriteLine("Server stopped");
            }
        }
    }
}
