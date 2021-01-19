using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Factories
{
    public interface ICharacterRendererFactory
    {
        ICharacterRenderer CreateCharacterRenderer(ICharacter character);
    }
}
