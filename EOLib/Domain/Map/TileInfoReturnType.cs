// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Map
{
    public enum TileInfoReturnType
    {
        IsTileSpec, //indicates that a normal tile spec is returned
        IsWarpSpec, //indicates that a normal warp spec is returned
        IsOtherPlayer, //other player is in the way, spec/warp are invalid
        IsOtherNPC, //other npc is in the way, spec/warp are invalid
        IsMapSign
    }
}
