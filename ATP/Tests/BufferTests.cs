﻿using ATP;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class BufferTests
    {
        private InfiniteCircularBuffer buffer;

        [SetUp]
        public void SetUp()
        {
            buffer = new InfiniteCircularBuffer(8);
        }

        [Test]
        public void AddGet()
        {
            var ret = buffer.TryAdd(new byte[] {5, 6, 7}, 0, 3, 0);
            var ret2 = buffer.TryAdd(new byte[] {9, 10, 11}, 0, 3, 3);

            byte[] res = new byte[4];
            var ret3 = buffer.TryGet(res, 4, 1);

            Assert.True(ret);
            Assert.True(ret2);

            Assert.True(ret3);

            Assert.AreEqual(6, res[0]);
            Assert.AreEqual(7, res[1]);
            Assert.AreEqual(9, res[2]);
            Assert.AreEqual(10, res[3]);
        }

        [Test]
        public void AddDisposeGet()
        {
            var ret = buffer.TryAdd(new byte[] {5, 6, 7}, 0, 3, 0);
            var ret2 = buffer.TryAdd(new byte[] {9, 10, 11}, 0, 3, 3);

            buffer.DisposeElements(1, 1);
            buffer.DisposeElements(0, 1);

            var ret3 = buffer.TryAdd(new byte[] {42, 43, 44}, 0, 3, 6);

            byte[] res = new byte[3];
            var ret4 = buffer.TryGet(res, 3, 6);

            Assert.True(ret);
            Assert.True(ret2);
            Assert.True(ret3);

            Assert.True(ret4);

            Assert.AreEqual(42, res[0]);
            Assert.AreEqual(43, res[1]);
            Assert.AreEqual(44, res[2]);
        }

        [Test]
        public void Overweite()
        {
            var ret = buffer.TryAdd(new byte[] {5, 6, 7}, 0, 3, 0);
            var ret2 = buffer.TryAdd(new byte[] {9, 10, 11}, 0, 3, 2);

            byte[] res = new byte[3];
            var ret3 = buffer.TryGet(res, 3, 1);

            Assert.True(ret);
            Assert.True(ret2);

            Assert.True(ret3);

            Assert.AreEqual(6, res[0]);
            Assert.AreEqual(9, res[1]);
            Assert.AreEqual(10, res[2]);
        }
    }
}
