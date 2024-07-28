using AutomaticTypeMapper;
using EOLib.IO.Map;
using System.Collections.Generic;

namespace EOLib.IO.Repositories
{
    public interface IMapFileRepository
    {
        Dictionary<int, IMapFile> MapFiles { get; }
    }

    public interface IMapFileProvider
    {
        IReadOnlyDictionary<int, IMapFile> MapFiles { get; }
    }

    [MappedType(BaseType = typeof(IMapFileRepository), IsSingleton = true)]
    [MappedType(BaseType = typeof(IMapFileProvider), IsSingleton = true)]
    public class MapFileRepository : IMapFileRepository, IMapFileProvider
    {
        private readonly Dictionary<int, IMapFile> _mapCache;

        public Dictionary<int, IMapFile> MapFiles => _mapCache;

        IReadOnlyDictionary<int, IMapFile> IMapFileProvider.MapFiles => _mapCache;

        public MapFileRepository()
        {
            _mapCache = new Dictionary<int, IMapFile>();
        }
    }
}