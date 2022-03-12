using System.Collections.Generic;
using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public interface IPubFile<out TRecord> : IPubFile
        where TRecord : class, IPubRecord, new()
    {
        TRecord this[int id] { get; }

        IReadOnlyList<TRecord> Data { get; }
    }

    public interface IPubFile
    {
        string FileType { get; }

        int CheckSum { get; set; }

        int Length { get; }

        byte[] SerializeToByteArray(INumberEncoderService numberEncoderService, bool rewriteChecksum = true);

        void DeserializeFromByteArray(byte[] bytes, INumberEncoderService numberEncoderService);
    }
}
