using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public interface IPubFile<TRecord> : IPubFile, IEnumerable<TRecord>
        where TRecord : class, IPubRecord, new()
    {
        /// <summary>
        /// Get the record at the corresponding record ID (1-based: the first record is at ID 1).
        /// </summary>
        /// <param name="id">The ID of the record to find.</param>
        /// <returns>The record.</returns>
        TRecord this[int id] { get; }

        /// <summary>
        /// Create a copy of this pub file with a newly added record.
        /// </summary>
        /// <param name="record">The record to add.</param>
        /// <returns>The updated pub file.</returns>
        IPubFile<TRecord> WithAddedRecord(TRecord record);

        /// <summary>
        /// Create a copy of this pub file with a record inserted at the ID specified by record.ID. Updates all record IDs in the file to match their corresponding index (1-based).
        /// </summary>
        /// <param name="record">The record to insert.</param>
        /// <returns>The updated pub file.</returns>
        IPubFile<TRecord> WithInsertedRecord(TRecord record);

        /// <summary>
        /// Create a copy of this pub file with a record updated at the ID specified by record.ID.
        /// </summary>
        /// <param name="record">The record to insert.</param>
        /// <returns>The updated pub file.</returns>
        IPubFile<TRecord> WithUpdatedRecord(TRecord record);

        /// <summary>
        /// Create a copy of this pub file with a record removed at the ID specified by record.ID. Updates all record IDs in the file to match their corresponding index (1-based).
        /// </summary>
        /// <param name="record">The record to insert.</param>
        /// <returns>The updated pub file.</returns>
        IPubFile<TRecord> WithRemovedRecord(TRecord record);
    }

    public interface IPubFile
    {
        /// <summary>
        /// The type of the file, usually a 3-character string e.g. EIF/ENF/ESF/ECF
        /// </summary>
        string FileType { get; }

        /// <summary>
        /// The file checksum. This is really two shorts encoded next to each other but is represented as an int.
        /// </summary>
        int CheckSum { get; }

        /// <summary>
        /// The length of the file (number of records)
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Create a copy of this pub file with the specified checksum value.
        /// </summary>
        /// <param name="checkSum">The new checksum.</param>
        /// <returns>The updated pub file.</returns>
        IPubFile WithCheckSum(int checkSum);
    }
}
