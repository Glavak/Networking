using System;

namespace ATP
{
    public class InfiniteCircularBuffer
    {
        public int Size => data.Length;

        private byte[] data;
        private bool[] validData;
        private long firstElement = 0;
        private long firstElementAbsolutePosition = 0;
        private long lastElement = 0;

        private long elementsCount => (Size + lastElement - firstElement + 1) % Size;

        public InfiniteCircularBuffer(int size)
        {
            data = new byte[size];
            validData = new bool[size];
        }

        public bool TryAdd(byte[] dataToAdd, int offset, int count, long absolutePositionToAdd)
        {
            if (count > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "count can't be greater that buffer size");
            }

            if (count + absolutePositionToAdd - firstElementAbsolutePosition > Size)
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                var indexInData = GetIndexInData(absolutePositionToAdd + i);

                data[indexInData] = dataToAdd[offset + i];
                validData[indexInData] = true;

                lastElement = indexInData;
            }

            return true;
        }

        public bool TryGet(byte[] buffer, int count, long absolutePositionToGet)
        {
            if (absolutePositionToGet < firstElementAbsolutePosition)
            {
                throw new ArgumentOutOfRangeException(nameof(absolutePositionToGet), "no valid data at this position");
            }

            if (absolutePositionToGet + count > firstElementAbsolutePosition + elementsCount)
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                var indexInData = GetIndexInData(absolutePositionToGet + i);

                if (!validData[indexInData]) return false;
            }

            for (int i = 0; i < count; i++)
            {
                var indexInData = GetIndexInData(absolutePositionToGet + i);

                buffer[i] = data[indexInData];
                validData[indexInData] = false;
            }

            return true;
        }

        public void DisposeElements(long absolutePosition, int count)
        {
            if (absolutePosition < firstElementAbsolutePosition)
            {
                throw new ArgumentOutOfRangeException(nameof(absolutePosition), "no valid data at this position");
            }

            if (absolutePosition + count > firstElementAbsolutePosition + elementsCount)
            {
                throw new ArgumentOutOfRangeException(nameof(absolutePosition), "no valid data at this position");
            }

            for (int i = 0; i < count; i++)
            {
                var indexInData = GetIndexInData(absolutePosition + i);

                validData[indexInData] = false;
            }

            if (absolutePosition == firstElementAbsolutePosition)
            {
                firstElement = (firstElement + count) % data.Length;
                firstElementAbsolutePosition += count;
            }

            while (validData[firstElement] == false)
            {
                firstElement = (firstElement + 1) % data.Length;
                firstElementAbsolutePosition++;
            }
        }

        private long GetIndexInData(long absolutePosition)
        {
            if (absolutePosition < firstElementAbsolutePosition)
            {
                throw new ArgumentOutOfRangeException(nameof(absolutePosition));
            }

            if (absolutePosition >= firstElementAbsolutePosition + Size)
            {
                throw new ArgumentOutOfRangeException(nameof(absolutePosition));
            }

            var indexInData = firstElement + absolutePosition - firstElementAbsolutePosition;
            return indexInData % data.Length;
        }
    }
}
