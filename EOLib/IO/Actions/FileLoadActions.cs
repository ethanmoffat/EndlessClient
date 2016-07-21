// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using EOLib.Config;
using EOLib.IO.Repositories;
using EOLib.IO.Services;

namespace EOLib.IO.Actions
{
    public class FileLoadActions : IFileLoadActions
    {
        private readonly IMapFileRepository _mapFileRepository;
        private readonly IDataFileRepository _dataFileRepository;
        private readonly IConfigurationRepository _configRepository;
        private readonly IMapFileLoadService _mapFileLoadService;

        public FileLoadActions(IMapFileRepository mapFileRepository,
                               IDataFileRepository dataFileRepository,
                               IConfigurationRepository configRepository,
                               IMapFileLoadService mapFileLoadService)
        {
            _mapFileRepository = mapFileRepository;
            _dataFileRepository = dataFileRepository;
            _configRepository = configRepository;
            _mapFileLoadService = mapFileLoadService;
        }

        public void LoadMapFileByID(int id)
        {
            LoadMapFileByName(string.Format(Constants.MapFileFormatString, id));
        }

        public void LoadMapFileByName(string fileName)
        {
            var mapFile = _mapFileLoadService.LoadMapByPath(fileName);
            if (_mapFileRepository.MapFiles.ContainsKey(mapFile.Properties.MapID))
                _mapFileRepository.MapFiles[mapFile.Properties.MapID] = mapFile;
            else
                _mapFileRepository.MapFiles.Add(mapFile.Properties.MapID, mapFile);
        }

        public void LoadDataFiles()
        {
            var files = Directory.GetFiles(Constants.DataFilePath, "*.edf");

            if (!Directory.Exists(Constants.DataFilePath) ||
                files.Length != Constants.ExpectedNumberOfDataFiles)
                throw new DataFileLoadException();

            _dataFileRepository.DataFiles.Clear();
            for (int i = 1; i <= Constants.ExpectedNumberOfDataFiles; ++i)
            {
                if (!DataFileNameIsValid(i, files[i-1]))
                    throw new DataFileLoadException();

                var fileToLoad = (DataFiles) i;
                var loadedFile = new EDFFile(files[i-1], fileToLoad);
                
                _dataFileRepository.DataFiles.Add(fileToLoad, loadedFile);
            }
        }

        private bool DataFileNameIsValid(int fileNumber, string fileName)
        {
            var expectedFormat = string.Format("data/dat0{0:00}.edf", fileNumber);
            return expectedFormat == fileName;
        }
    }
}
