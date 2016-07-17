// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Map;

namespace EOLib.Domain.Map
{
    public interface ITileInfo
    {
        TileInfoReturnType ReturnType { get; }

        TileSpec Spec { get; }

        IMapElement MapElement { get; }
    }
}
