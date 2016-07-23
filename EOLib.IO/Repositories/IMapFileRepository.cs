// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.IO.Map;

namespace EOLib.IO.Repositories
{
    public interface IMapFileRepository
    {
        Dictionary<int, IReadOnlyMapFile> MapFiles { get; }
    }

    public interface IMapFileProvider
    {
        IReadOnlyDictionary<int, IReadOnlyMapFile> MapFiles { get; }
    }

    public class MapFileRepository : IMapFileRepository, IMapFileProvider
    {
        private readonly Dictionary<int, IReadOnlyMapFile> _mapCache;

        public Dictionary<int, IReadOnlyMapFile> MapFiles { get { return _mapCache; } }

        IReadOnlyDictionary<int, IReadOnlyMapFile> IMapFileProvider.MapFiles
        {
            get { return _mapCache; }
        }

        public MapFileRepository()
        {
            _mapCache = new Dictionary<int, IReadOnlyMapFile>();
        }
    }
}