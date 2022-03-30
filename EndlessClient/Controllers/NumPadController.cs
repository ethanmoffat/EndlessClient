using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class NumPadController : INumPadController
    {
        private readonly ICharacterActions _characterActions;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;

        public NumPadController(ICharacterActions characterActions,
                                IHudControlProvider hudControlProvider,
                                ICharacterRendererProvider characterRendererProvider)
        {
            _characterActions = characterActions;
            _hudControlProvider = hudControlProvider;
            _characterRendererProvider = characterRendererProvider;
        }

        public void Emote(Emote whichEmote)
        {
            var mainRenderer = _characterRendererProvider.MainCharacterRenderer;
            mainRenderer.MatchSome(renderer =>
            {
                if (renderer.Character.RenderProperties.IsActing(CharacterActionState.Emote))
                    return;

                _characterActions.Emote(whichEmote);

                var animator = _hudControlProvider.GetComponent<ICharacterAnimator>(HudControlIdentifier.CharacterAnimator);
                animator.Emote(renderer.Character.ID, whichEmote);
            });
        }
    }

    public interface INumPadController
    {
        void Emote(Emote whichEmote);
    }
}
