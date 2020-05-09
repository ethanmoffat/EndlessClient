using System.Collections.Generic;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.CharacterProperties
{
    public interface ICharacterPropertyRendererBuilder
    {
        IEnumerable<ICharacterPropertyRenderer> BuildList(ICharacterTextures characterTextures,
                                                          ICharacterRenderProperties renderProperties);
    }
}