using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public interface IPubFile<TRecord> : IPubFile, IEnumerable<TRecord>
        where TRecord : class, IPubRecord, new()
    {
        TRecord this[int id] { get; }

        IPubFile<TRecord> WithAddedRecord(TRecord record);

        IPubFile<TRecord> WithUpdatedRecord(TRecord record);

        IPubFile<TRecord> WithRemovedRecord(TRecord record);
    }

    public interface IPubFile
    {
        string FileType { get; }

        int CheckSum { get; }

        int Length { get; }

        IPubFile WithCheckSum(int checkSum);
    }
}
