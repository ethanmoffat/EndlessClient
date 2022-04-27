using AutomaticTypeMapper;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class NumPadController : INumPadController
    {
        private readonly ICharacterActions _characterActions;
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly ICharacterProvider _characterProvider;

        public NumPadController(ICharacterActions characterActions,
                                ICharacterAnimationActions characterAnimationActions,
                                ICharacterProvider characterProvider)
        {
            _characterActions = characterActions;
            _characterAnimationActions = characterAnimationActions;
            _characterProvider = characterProvider;
        }

        public void Emote(Emote whichEmote)
        {
            if (!_characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Standing))
                return;

            _characterActions.Emote(whichEmote);
            _characterAnimationActions.Emote(whichEmote);
        }
    }

    public interface INumPadController
    {
        void Emote(Emote whichEmote);
    }
}
