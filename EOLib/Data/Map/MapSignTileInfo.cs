// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Map;

namespace EOLib.Data.Map
{
	public class MapSignTileInfo : ITileInfo
	{
		public TileInfoReturnType ReturnType { get { return TileInfoReturnType.IsMapSign; } }
		public TileSpec Spec { get { return TileSpec.None; } }
		public IMapElement MapElement { get; private set; }

		public MapSignTileInfo(MapSign mapSign)
		{
			MapElement = mapSign;
		}
	}
}
