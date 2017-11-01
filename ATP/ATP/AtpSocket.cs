using System;
using System.Threading;

namespace ATP
{
    public abstract class AtpSocket : IDisposable
    {
        internal InfiniteCircularBuffer SendBuffer = new InfiniteCircularBuffer(1024);
        internal InfiniteCircularBuffer RecieveBuffer = new InfiniteCircularBuffer(1024);

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
                //TODO: get less than count, if availiable
                while (!RecieveBuffer.TryGetFromBegin(buff, count))
                {
                    Monitor.Wait(RecieveBuffer);
                }

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
