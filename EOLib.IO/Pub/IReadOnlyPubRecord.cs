// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public interface IReadOnlyPubRecord
    {
        int RecordSize { get; }

        int ID { get; }

        string Name { get; }

        TValue Get<TValue>(PubRecordPropertyType type);

        byte[] SerializeToByteArray(INumberEncoderService numberEncoderService);
    }
}