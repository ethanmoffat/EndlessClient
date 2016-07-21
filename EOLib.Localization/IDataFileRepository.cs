// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Localization;

namespace EOLib.IO.Repositories
{
    public interface IDataFileRepository
    {
        Dictionary<DataFiles, EDFFile> DataFiles { get; }
    }

    public interface IDataFileProvider
    {
        IReadOnlyDictionary<DataFiles, EDFFile> DataFiles { get; }
    }

    public class DataFileRepository : IDataFileRepository, IDataFileProvider
    {
        private readonly Dictionary<DataFiles, EDFFile> _dataFiles;

        public Dictionary<DataFiles, EDFFile> DataFiles { get { return _dataFiles; } }
        IReadOnlyDictionary<DataFiles, EDFFile> IDataFileProvider.DataFiles { get { return _dataFiles; } }

        public DataFileRepository()
        {
            _dataFiles = new Dictionary<DataFiles, EDFFile>(DataConstants.ExpectedNumberOfDataFiles);
        }
    }
}
