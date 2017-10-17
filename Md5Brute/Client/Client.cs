using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Model;

namespace Client
{
    public class Client
    {
        private CalcType type = CalcType.ExactlyAsSpecified;
        private int charsToCalc;
        private string prefix;

        private byte[] desiredHash;

        private string result;

        private readonly HashAlgorithm algorithm;
        private readonly Guid id = Guid.NewGuid();

        public bool IsFinished;

        public Client()
        {
            algorithm = new MD5Cng();
        }

        public void GetJob(IPAddress address, int port)
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(address, port);

                BinaryWriter writer = new BinaryWriter(client.GetStream());
                writer.Write((byte) RequestOpcode.JobRequest);
                writer.Write(id.ToByteArray());

                BinaryReader reader = new BinaryReader(client.GetStream());

                charsToCalc = reader.ReadInt32();
                if (charsToCalc == 0)
                {
                    // Hash already found
                    IsFinished = true;
                }
                else if (charsToCalc < 0)
                {
                    type = CalcType.LessThanSpecified;
                    charsToCalc = -charsToCalc;
                }

                prefix = reader.ReadString();

                var hashLength = reader.ReadInt32();
                desiredHash = reader.ReadBytes(hashLength);
            }

            string stars = string.Empty;
            for (int i = 0; i < charsToCalc; i++) stars += "*";
            Console.WriteLine($"Got job! {prefix}{stars}");
        }

        public void FindHash()
        {
            if (IsFinished) return;

            switch (type)
            {
                case CalcType.ExactlyAsSpecified:
                    CheckStrings(charsToCalc);
                    break;

                case CalcType.LessThanSpecified:
                    for (int i = 0; i < charsToCalc; i++)
                    {
                        CheckStrings(i);

                        if(IsFinished) break;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SubmitResult(IPAddress address, int port)
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(address, port);

                BinaryWriter writer = new BinaryWriter(client.GetStream());

                if (result == null)
                {
                    writer.Write((byte) RequestOpcode.HashNotFound);
                    writer.Write(id.ToByteArray());
                }
                else
                {
                    writer.Write((byte) RequestOpcode.HashFound);
                    writer.Write(result);
                }
            }
        }

        private void CheckStrings(int stringsLength)
        {
            char[] suffix = Transmutations.First(stringsLength);

            do
            {
                string str = prefix + new string(suffix);
                if (prefix == "AAAAA")
                    Console.WriteLine($"Found right string! Result: {result}");
                byte[] bytes = Encoding.UTF8.GetBytes(str);

                byte[] hash = algorithm.ComputeHash(bytes);

                if (IsRightHash(hash))
                {
                    result = str;
                    IsFinished = true;

                    Console.WriteLine($"Found right string! Result: {result}");
                    return;
                }
            } while (Transmutations.Next(suffix));
        }

        private bool IsRightHash(byte[] hash)
        {
            for (int i = 0; i < hash.Length; i++)
            {
                if (hash[i] != desiredHash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
