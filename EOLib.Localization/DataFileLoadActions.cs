using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AutomaticTypeMapper;
using EOLib.Shared;

namespace EOLib.Localization
{
    [MappedType(BaseType = typeof(IDataFileLoadActions))]
    public class DataFileLoadActions : IDataFileLoadActions
    {
        private readonly IDataFileRepository _dataFileRepository;
        private readonly IEDFLoaderService _edfLoaderService;

        public DataFileLoadActions(IDataFileRepository dataFileRepository,
                                   IEDFLoaderService edfLoaderService)
        {
            _dataFileRepository = dataFileRepository;
            _edfLoaderService = edfLoaderService;
        }

        public void LoadDataFiles()
        {
            if (!Directory.Exists(Constants.DataFilePath))
                throw new DataFileLoadException();

            var files = Directory.GetFiles(Constants.DataFilePath, "*.edf")
                                 .OrderBy(x => x)
                                 .ToArray();
            if (files.Length != Constants.ExpectedNumberOfDataFiles)
                throw new DataFileLoadException();

            _dataFileRepository.DataFiles.Clear();
            for (int i = 1; i <= Constants.ExpectedNumberOfDataFiles; ++i)
            {
                if (!DataFileNameIsValid(i, files[i - 1]))
                    throw new DataFileLoadException();

                var fileToLoad = (DataFiles)i;
                _dataFileRepository.DataFiles[fileToLoad] = _edfLoaderService.LoadFile(files[i - 1], fileToLoad);
            }
        }

        private bool DataFileNameIsValid(int fileNumber, string fileName)
        {
            var expectedFormat = string.Format(Constants.DataFileFormat, fileNumber).Replace('/', Path.DirectorySeparatorChar);
            return expectedFormat == fileName;
        }
    }

    public interface IDataFileLoadActions
    {
        void LoadDataFiles();
    }
}
