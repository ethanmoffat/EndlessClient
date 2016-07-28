// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;

namespace EndlessClient.Rendering.CharacterProperties
{
    public interface ICharacterRenderOffsetCalculator
    {
        int CalculateOffsetX(ICharacter character);

        int CalculateOffsetY(ICharacter character);
    }
}
