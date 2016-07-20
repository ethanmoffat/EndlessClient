// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public interface IReadOnlyPubFile<out TRecord> : IReadOnlyPubFile
        where TRecord : IPubRecord, new()
    {
        TRecord this[int id] { get; }

        byte[] SerializeToByteArray(INumberEncoderService numberEncoderService);
    }

    public interface IReadOnlyPubFile
    {
        string FileType { get; }

        int CheckSum { get; }

        int Length { get; }
    }
}
