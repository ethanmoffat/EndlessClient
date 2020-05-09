using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Factories
{
    public interface ICharacterRendererFactory
    {
        //this will eventually need to support IDs and do a lookup globally for the correct render properties
        ICharacterRenderer CreateCharacterRenderer(ICharacterRenderProperties initialRenderProperties);
    }
}
