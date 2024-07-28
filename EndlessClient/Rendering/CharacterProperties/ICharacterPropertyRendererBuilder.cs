using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;
using System.Collections.Generic;

namespace EndlessClient.Rendering.CharacterProperties
{
    public interface ICharacterPropertyRendererBuilder
    {
        IEnumerable<ICharacterPropertyRenderer> BuildList(ICharacterTextures characterTextures,
                                                          CharacterRenderProperties renderProperties);
    }
}