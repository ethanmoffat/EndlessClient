// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO
{
    public interface IDataRecord
    {
        int ID { get; set; }

        string Name { get; }

        /// <summary>
        /// The number of 'names' this data record has (usually 1)
        /// </summary>
        int NameCount { get; }

        /// <summary>
        /// Set the 'names' for this data record
        /// </summary>
        /// <param name="names">The names that were read from the file</param>
        void SetNames(params string[] names);

        byte[] SerializeToByteArray();

        void DeserializeFromByteArray(int version, byte[] rawData);

        // Require implementation of ToString() for debugging purposes
        string ToString();
    }
}