using System;
using System.Threading;

namespace ATP
{
    public abstract class AtpSocket : IDisposable
    {
        internal InfiniteCircularBuffer SendBuffer = new InfiniteCircularBuffer(1024);
        internal InfiniteCircularBuffer RecieveBuffer = new InfiniteCircularBuffer(1024);

        internal DateTime LastSend = DateTime.MinValue;
        internal DateTime LastRecieved = DateTime.MinValue;

        public readonly int SendBy = 64;

        public virtual void Send(byte[] buff, int offset, int count)
        {
            lock (SendBuffer)
            {
                while (!SendBuffer.TryAddToEnd(buff, offset, count))
                {
                    Monitor.Wait(SendBuffer);
                }

                Monitor.PulseAll(SendBuffer);
            }
        }

        public virtual int Recieve(byte[] buff, int count)
        {
            lock (RecieveBuffer)
            {
                int bytesAvailiable;
                do
                {
                    bytesAvailiable = RecieveBuffer.GetAvailibleBytesAtBegin();
                    if (bytesAvailiable == 0)
                    {
                        Monitor.Wait(RecieveBuffer);
                    }
                } while (bytesAvailiable == 0);

                if (bytesAvailiable < count) count = bytesAvailiable;

                RecieveBuffer.TryGetFromBegin(buff, count);
                RecieveBuffer.DisposeElementsAtBegin(count);

                Monitor.PulseAll(RecieveBuffer);
            }

            return count;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}
