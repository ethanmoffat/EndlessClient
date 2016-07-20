// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public interface IPubRecord
    {
        int RecordSize { get; }

        int ID { get; set; }

        string Name { get; set; }

        TValue Get<TValue>(PubRecordProperty type);

        byte[] SerializeToByteArray(INumberEncoderService numberEncoderService);

        void DeserializeFromByteArray(byte[] recordBytes, INumberEncoderService numberEncoderService);
    }
}