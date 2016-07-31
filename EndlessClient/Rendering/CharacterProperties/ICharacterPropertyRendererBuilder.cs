// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.CharacterProperties
{
    public interface ICharacterPropertyRendererBuilder
    {
        List<ICharacterPropertyRenderer> BuildList(ICharacterTextures characterTextures,
                                                   ICharacterRenderProperties renderProperties);
    }
}