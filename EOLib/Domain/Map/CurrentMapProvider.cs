using AutomaticTypeMapper;
using EOLib.IO.Map;
using EOLib.IO.Repositories;

namespace EOLib.Domain.Map
{
    [AutoMappedType(IsSingleton = true)]
    public class CurrentMapProvider : ICurrentMapProvider
    {
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IMapFileProvider _mapFileProvider;

        public CurrentMapProvider(ICurrentMapStateProvider currentMapStateProvider,
                                  IMapFileProvider mapFileProvider)
        {
            _currentMapStateProvider = currentMapStateProvider;
            _mapFileProvider = mapFileProvider;
        }

        public IMapFile CurrentMap => _mapFileProvider.MapFiles[_currentMapStateProvider.CurrentMapID];
    }

    public interface ICurrentMapProvider
    {
        IMapFile CurrentMap { get; }
    }
}
