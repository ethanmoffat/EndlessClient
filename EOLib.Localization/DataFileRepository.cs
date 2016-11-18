// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.Localization
{
    public class DataFileRepository : IDataFileRepository, IDataFileProvider
    {
        private readonly Dictionary<DataFiles, IEDFFile> _dataFiles;

        public Dictionary<DataFiles, IEDFFile> DataFiles { get { return _dataFiles; } }
        IReadOnlyDictionary<DataFiles, IEDFFile> IDataFileProvider.DataFiles { get { return _dataFiles; } }

        public DataFileRepository()
        {
            _dataFiles = new Dictionary<DataFiles, IEDFFile>(DataFileConstants.ExpectedNumberOfDataFiles);
        }
    }

    public interface IDataFileRepository
    {
        Dictionary<DataFiles, IEDFFile> DataFiles { get; }
    }

    public interface IDataFileProvider
    {
        IReadOnlyDictionary<DataFiles, IEDFFile> DataFiles { get; }
    }
}
