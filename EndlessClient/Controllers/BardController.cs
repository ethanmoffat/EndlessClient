using AutomaticTypeMapper;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Interact.Jukebox;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class BardController : IBardController
    {
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly IJukeboxActions _jukeboxActions;
        private readonly ICharacterProvider _characterProvider;

        public BardController(ICharacterAnimationActions characterAnimationActions,
                              IJukeboxActions jukeboxActions,
                              ICharacterProvider characterProvider)
        {
            _characterAnimationActions = characterAnimationActions;
            _jukeboxActions = jukeboxActions;
            _characterProvider = characterProvider;
        }

        public void PlayInstrumentNote(int noteIndex)
        {
            _characterAnimationActions.StartAttacking(noteIndex);
            _jukeboxActions.PlayNote(noteIndex);
        }

        private bool CanAttackAgain()
        {
            var rp = _characterProvider.MainCharacter.RenderProperties;
            return rp.IsActing(CharacterActionState.Standing) ||
                   rp.RenderAttackFrame == CharacterRenderProperties.MAX_NUMBER_OF_ATTACK_FRAMES;
        }
    }

    public interface IBardController
    {
        void PlayInstrumentNote(int noteIndex);
    }
}
