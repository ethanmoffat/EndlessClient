using AutomaticTypeMapper;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using EOLib.IO.Services;

namespace EOLib.IO.Actions
{
    [MappedType(BaseType = typeof(IMapFileLoadActions))]
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
            var mapFile = _mapFileLoadService.LoadMapByID(id);
            AddMapToCache(mapFile);
        }

        public void LoadMapFileByName(string fileName)
        {
            var mapFile = _mapFileLoadService.LoadMapByPath(fileName);
            AddMapToCache(mapFile);
        }

        private void AddMapToCache(IMapFile mapFile)
        {
            if (_mapFileRepository.MapFiles.ContainsKey(mapFile.Properties.MapID))
                _mapFileRepository.MapFiles[mapFile.Properties.MapID] = mapFile;
            else
                _mapFileRepository.MapFiles.Add(mapFile.Properties.MapID, mapFile);
        }
    }
}
