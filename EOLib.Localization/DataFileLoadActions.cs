using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AutomaticTypeMapper;

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
            if (!Directory.Exists(GetPath(DataFileConstants.DataFilePath)))
                throw new DataFileLoadException();

            var files = Directory.GetFiles(GetPath(DataFileConstants.DataFilePath), "*.edf")
                                 .OrderBy(x => x)
                                 .ToArray();
            if (files.Length != DataFileConstants.ExpectedNumberOfDataFiles)
                throw new DataFileLoadException();

            _dataFileRepository.DataFiles.Clear();
            for (int i = 1; i <= DataFileConstants.ExpectedNumberOfDataFiles; ++i)
            {
                if (!DataFileNameIsValid(i, files[i - 1]))
                    throw new DataFileLoadException();

                var fileToLoad = (DataFiles)i;
                _dataFileRepository.DataFiles[fileToLoad] = _edfLoaderService.LoadFile(files[i - 1], fileToLoad);
            }
        }

        private bool DataFileNameIsValid(int fileNumber, string fileName)
        {
            var expectedFormat = GetPath($"data/dat0{fileNumber:00}.edf");
            return expectedFormat == fileName;
        }

        private string GetPath(string inputPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine("Contents", "Resources", inputPath);
            }
            else
            {
                return inputPath;
            }
        }
    }

    public interface IDataFileLoadActions
    {
        void LoadDataFiles();
    }
}
