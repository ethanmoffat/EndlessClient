// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Map;
using EOLib.IO.Repositories;

namespace EOLib.Domain.Map
{
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


        public IMapFile CurrentMap
        {
            get { return _mapFileProvider.MapFiles[_currentMapStateProvider.CurrentMapID]; }
        }
    }

    public interface ICurrentMapProvider
    {
        IMapFile CurrentMap { get; }
    }
}
