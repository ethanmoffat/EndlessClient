using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Map;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Notifiers;

namespace EndlessClient.Rendering.Character
{
    [MappedType(BaseType = typeof(ICharacterAnimationActions))]
    [MappedType(BaseType = typeof(IOtherCharacterAnimationNotifier))]
    public class CharacterAnimationActions : ICharacterAnimationActions, IOtherCharacterAnimationNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICharacterRepository _characterRepository;
        private readonly ISpikeTrapActions _spikeTrapActions;

        public CharacterAnimationActions(IHudControlProvider hudControlProvider,
                                         ICharacterRepository characterRepository,
                                         ISpikeTrapActions spikeTrapActions)
        {
            _hudControlProvider = hudControlProvider;
            _characterRepository = characterRepository;
            _spikeTrapActions = spikeTrapActions;
        }

        public void Face(EODirection direction)
        {
            var renderProperties = _characterRepository.MainCharacter.RenderProperties;
            renderProperties = renderProperties.WithDirection(direction);

            var newMainCharacter = _characterRepository.MainCharacter.WithRenderProperties(renderProperties);
            _characterRepository.MainCharacter = newMainCharacter;
        }

        public void StartWalking()
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartMainCharacterWalkAnimation();
        }

        public void StartAttacking()
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartMainCharacterAttackAnimation();
        }

        public void StartOtherCharacterWalkAnimation(int characterID)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartOtherCharacterWalkAnimation(characterID);

            _spikeTrapActions.HideSpikeTrap(characterID);
            _spikeTrapActions.ShowSpikeTrap(characterID);
        }

        public void StartOtherCharacterAttackAnimation(int characterID)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartOtherCharacterAttackAnimation(characterID);
        }

        private ICharacterAnimator Animator => _hudControlProvider.GetComponent<ICharacterAnimator>(HudControlIdentifier.CharacterAnimator);
    }

    public interface ICharacterAnimationActions
    {
        void Face(EODirection direction);

        void StartWalking();

        void StartAttacking();
    }
}
