// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Pub
{
    public interface IPubFile<TRecord> where TRecord : IPubRecord
    {
        string FileType { get; }

        int CheckSum { get; set; }

        int Length { get; }

        int Version { get; set; }

        TRecord this[int id] { get; set; }

        byte[] SerializeToByteArray();

        void CreateFromByteArray(byte[] bytes);
    }
}
