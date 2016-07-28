// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;
using EOLib.IO.Map;

namespace EndlessClient.Rendering.Map
{
    public interface IMapRenderDistanceCalculator
    {
        MapRenderBounds CalculateRenderBounds(ICharacter character, IReadOnlyMapFile currentMap);
    }
}
