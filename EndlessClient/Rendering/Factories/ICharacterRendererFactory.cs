using EndlessClient.Rendering.Character;

namespace EndlessClient.Rendering.Factories
{
    public interface ICharacterRendererFactory
    {
        ICharacterRenderer CreateCharacterRenderer(EOLib.Domain.Character.Character character);
    }
}
