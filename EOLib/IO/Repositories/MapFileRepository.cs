// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.IO.Map;

namespace EOLib.IO.Repositories
{
	public class MapFileRepository : IMapFileRepository, IMapFileProvider
	{
		private readonly Dictionary<int, IMapFile> _mapCache;

		public Dictionary<int, IMapFile> MapFiles { get { return _mapCache; } }

		IReadOnlyDictionary<int, IMapFile> IMapFileProvider.MapFiles
		{
			get { return _mapCache; }
		}

		public MapFileRepository()
		{
			_mapCache = new Dictionary<int, IMapFile>();
		}
	}
}