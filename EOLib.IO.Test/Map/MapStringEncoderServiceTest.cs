// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using EOLib.IO.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EOLib.IO.Test.Map
{
    [TestClass, ExcludeFromCodeCoverage]
    public class MapStringEncoderServiceTest
    {
        private IMapStringEncoderService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _service = new MapStringEncoderService();
        }

        [TestMethod]
        public void EncodeThenDecode_ReturnsOriginalString()
        {
            const string expected = "Test map string to encode";

            var bytes = _service.EncodeMapString(expected);
            var actual = _service.DecodeMapString(bytes);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncodeString_ReturnsExpectedBytes_FromKnownString()
        {
            var name = "Aeven" + Encoding.ASCII.GetString(Enumerable.Repeat((byte)0, 19).ToArray());
            var expectedBytes = new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 49, 104, 41, 104, 94
            };

            var actualBytes = _service.EncodeMapString(name);

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void DecodeString_ReturnsExpectedString_FromKnownBytes()
        {
            const string expected = "Aeven";

            var bytes = new byte[] {49, 104, 41, 104, 94};
            var fullBytes = Enumerable.Repeat((byte) 255, 24).ToArray();
            Array.Copy(bytes, 0, fullBytes, fullBytes.Length - bytes.Length, bytes.Length);

            var actual = _service.DecodeMapString(fullBytes);

            Assert.AreEqual(expected, actual);
        }
    }
}
