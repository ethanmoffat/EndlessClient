// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Repositories;
using EOLib.IO.Services;

namespace EOLib.IO.Actions
{
    public class MapFileLoadActions : IMapFileLoadActions
    {
        private readonly IMapFileRepository _mapFileRepository;
        private readonly IMapFileLoadService _mapFileLoadService;

        public MapFileLoadActions(IMapFileRepository mapFileRepository,
                               IMapFileLoadService mapFileLoadService)
        {
            _mapFileRepository = mapFileRepository;
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
    }
}
