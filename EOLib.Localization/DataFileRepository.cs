using AutomaticTypeMapper;
using System.Collections.Generic;

namespace EOLib.Localization
{
    [MappedType(BaseType = typeof(IDataFileRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IDataFileProvider), IsSingleton = true)]
    public class DataFileRepository : IDataFileRepository, IDataFileProvider
    {
        private readonly Dictionary<DataFiles, IEDFFile> _dataFiles;

        public Dictionary<DataFiles, IEDFFile> DataFiles => _dataFiles;
        IReadOnlyDictionary<DataFiles, IEDFFile> IDataFileProvider.DataFiles => _dataFiles;

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
