using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.CharacterProperties
{
    public interface ICharacterPropertyRendererBuilder
    {
        CharacterRenderLayers BuildLayers(ICharacterTextures characterTextures,
                                          CharacterRenderProperties renderProperties);
    }
}
