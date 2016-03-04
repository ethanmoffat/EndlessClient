// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Map;

namespace EOLib.Data.Map
{
	public class BasicTileInfo : ITileInfo
	{
		public TileInfoReturnType ReturnType { get; private set; }
		public virtual TileSpec Spec { get { return TileSpec.None; } }
		public IMapElement MapElement { get { return null; } }

		public BasicTileInfo(TileInfoReturnType returnType)
		{
			ReturnType = returnType;
		}
	}

	public class BasicTileInfoWithSpec : BasicTileInfo
	{
		private readonly TileSpec _spec;
		public override TileSpec Spec
		{
			get { return _spec; }
		}

		public BasicTileInfoWithSpec(TileSpec spec) 
			: base(TileInfoReturnType.IsTileSpec)
		{
			_spec = spec;
		}
	}
}
