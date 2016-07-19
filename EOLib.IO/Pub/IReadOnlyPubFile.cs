// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Pub
{
    public interface IReadOnlyPubFile<TRecord> where TRecord : IPubRecord
    {
        string FileType { get; }

        int CheckSum { get; }

        int Length { get; }

        int Version { get; }

        byte[] SerializeToByteArray();

        void CreateFromByteArray(byte[] bytes);
    }
}