// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO;

namespace EOLib.Data.Map
{
	public class WarpTileInfo : ITileInfo
	{
		public TileInfoReturnType ReturnType { get { return TileInfoReturnType.IsWarpSpec; } }
		public TileSpec Spec { get { return TileSpec.None; } }
		public IMapElement MapElement { get; private set; }

		public WarpTileInfo(Warp mapWarp)
		{
			MapElement = mapWarp;
		}
	}
}
