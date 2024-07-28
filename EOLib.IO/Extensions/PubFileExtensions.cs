using EOLib.IO.Pub;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.IO.Extensions
{
    public static class PubFileExtensions
    {
        /// <summary>
        /// Merge the set of pub files into a single pub file.
        /// <para>The first pub file has subsequent pub files' records added to the end of its set of records.</para>
        /// <para>This operation creates a deep copy.</para>
        /// </summary>
        /// <typeparam name="TRecord">The record type.</typeparam>
        /// <param name="pubFiles">The list of files to merge.</param>
        /// <returns>The merged pub file.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pubFiles"/> does not have at least one element.</exception>
        public static IPubFile<TRecord> Merge<TRecord>(IEnumerable<IPubFile<TRecord>> pubFiles)
            where TRecord : class, IPubRecord, new()
        {
            if (pubFiles.Count() < 1)
                throw new ArgumentException("pubFiles must have at least 1 element", nameof(pubFiles));

            if (pubFiles.Count() == 1)
                return pubFiles.ElementAt(0);

            var mergedFile = (IPubFile<TRecord>)pubFiles.ElementAt(0).Clone();

            var itemId = mergedFile.Length;
            foreach (var extraRecord in pubFiles.Skip(1).SelectMany(x => x))
                mergedFile = mergedFile.WithAddedRecord((TRecord)extraRecord.WithID(++itemId));

            return (IPubFile<TRecord>)mergedFile.WithTotalLength(pubFiles.Select(x => x.Length).Sum());
        }
    }
}