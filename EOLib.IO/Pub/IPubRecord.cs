// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public interface IPubRecord
    {
        int RecordSize { get; }

        int NameCount { get; }

        int ID { get; set; }

        string Name { get; set; }

        TValue Get<TValue>(PubRecordPropertyType type);

        byte[] SerializeToByteArray(INumberEncoderService numberEncoderService);

        void SetNames(string[] names);

        void DeserializeFromByteArray(byte[] bytes, INumberEncoderService numberEncoderService);
    }
}