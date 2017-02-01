// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using EOLib.IO.Pub;
using EOLib.IO.Services;

namespace EOLib.IO.Test.Pub
{
    [ExcludeFromCodeCoverage]
    internal class DummyRecord : IPubRecord
    {
        public int RecordSize => 0;

        public int ID { get; set; }

        public string Name { get; set; }

        public TValue Get<TValue>(PubRecordProperty type)
        {
            return default(TValue);
        }

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService)
        {
            var bytes = new byte[Name.Length + 1];
            bytes[0] = (byte) Name.Length;
            Array.Copy(Encoding.ASCII.GetBytes(Name), 0, bytes, 1, Name.Length);
            return bytes;
        }

        public void DeserializeFromByteArray(byte[] recordBytes, INumberEncoderService numberEncoderService)
        {
        }
    }
}
