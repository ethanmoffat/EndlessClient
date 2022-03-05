using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Map;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using System.Linq;

namespace EndlessClient.Rendering.Character
{
    [MappedType(BaseType = typeof(ICharacterAnimationActions))]
    [MappedType(BaseType = typeof(IOtherCharacterAnimationNotifier))]
    [MappedType(BaseType = typeof(IEffectNotifier))]
    public class CharacterAnimationActions : ICharacterAnimationActions, IOtherCharacterAnimationNotifier, IEffectNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ISpikeTrapActions _spikeTrapActions;
        private readonly IESFFileProvider _esfFileProvider;

        public CharacterAnimationActions(IHudControlProvider hudControlProvider,
                                         ICharacterRepository characterRepository,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         ICurrentMapProvider currentMapProvider,
                                         ISpikeTrapActions spikeTrapActions,
                                         IESFFileProvider esfFileProvider)
        {
            _hudControlProvider = hudControlProvider;
            _characterRepository = characterRepository;
            _currentMapStateProvider = currentMapStateProvider;
            _characterRendererProvider = characterRendererProvider;
            _currentMapProvider = currentMapProvider;
            _spikeTrapActions = spikeTrapActions;
            _esfFileProvider = esfFileProvider;
        }

        public void Face(EODirection direction)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.MainCharacterFace(direction);
        }

        public void StartWalking()
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartMainCharacterWalkAnimation();
            ShowWaterSplashiesIfNeeded(CharacterActionState.Walking,
                                       _characterRepository.MainCharacter,
                                       _characterRendererProvider.MainCharacterRenderer);
        }

        public bool IsAttacking(int characterId)
        {
            if (!_hudControlProvider.IsInGame)
                return false;

            return Animator.IsAttacking(characterId);
        }

        public void StartAttacking()
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartMainCharacterAttackAnimation();
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking,
                                       _characterRepository.MainCharacter,
                                       _characterRendererProvider.MainCharacterRenderer);
        }

        public void StartOtherCharacterWalkAnimation(int characterID)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartOtherCharacterWalkAnimation(characterID);
            ShowWaterSplashiesIfNeeded(CharacterActionState.Walking,
                                       _currentMapStateProvider.Characters.Single(x => x.ID == characterID),
                                       _characterRendererProvider.CharacterRenderers[characterID]);

            _spikeTrapActions.HideSpikeTrap(characterID);
            _spikeTrapActions.ShowSpikeTrap(characterID);
        }

        public void StartOtherCharacterAttackAnimation(int characterID)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartOtherCharacterAttackAnimation(characterID);
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking,
                                       _currentMapStateProvider.Characters.Single(x => x.ID == characterID),
                                       _characterRendererProvider.CharacterRenderers[characterID]);
        }

        public void NotifyWarpLeaveEffect(short characterId, WarpAnimation anim)
        {
            if (anim == WarpAnimation.Admin)
                _characterRendererProvider.CharacterRenderers[characterId].ShowWarpLeave();
        }

        public void NotifyWarpEnterEffect(short characterId, WarpAnimation anim)
        {
            if (anim == WarpAnimation.Admin)
            {
                if (!_characterRendererProvider.CharacterRenderers.ContainsKey(characterId))
                    _characterRendererProvider.NeedsWarpArriveAnimation.Add(characterId);
                else
                    _characterRendererProvider.CharacterRenderers[characterId].ShowWarpArrive();
            }
        }

        public void NotifyPotionEffect(short playerId, int effectId)
        {
            _characterRendererProvider.CharacterRenderers[playerId].ShowPotionAnimation(effectId);
        }

        public void NotifyStartSpellCast(short playerId, short spellId)
        {
            var shoutName = _esfFileProvider.ESFFile[spellId].Shout;
            _characterRendererProvider.CharacterRenderers[playerId].ShoutSpellPrep(shoutName.ToLower());
        }

        public void NotifySelfSpellCast(short playerId, short spellId, int spellHp, byte percentHealth)
        {
            var spellGraphic = _esfFileProvider.ESFFile[spellId].Graphic;

            if (playerId == _characterRepository.MainCharacter.ID)
            {
                _characterRendererProvider.MainCharacterRenderer.ShoutSpellCast();
                _characterRendererProvider.MainCharacterRenderer.ShowSpellAnimation(spellGraphic);
                _characterRendererProvider.MainCharacterRenderer.ShowDamageCounter(spellHp, percentHealth, isHeal: true);
            }
            else
            {
                Animator.StartOtherCharacterSpellCast(playerId);
                _characterRendererProvider.CharacterRenderers[playerId].ShoutSpellCast();
                _characterRendererProvider.CharacterRenderers[playerId].ShowSpellAnimation(spellGraphic);
                _characterRendererProvider.CharacterRenderers[playerId].ShowDamageCounter(spellHp, percentHealth, isHeal: true);
            }
        }

        public void NotifyTargetOtherSpellCast(short sourcePlayerID, short targetPlayerID, short spellId, int recoveredHP, byte targetPercentHealth)
        {
            var spellGraphic = _esfFileProvider.ESFFile[spellId].Graphic;

            if (sourcePlayerID == _characterRepository.MainCharacter.ID)
            {
                _characterRendererProvider.MainCharacterRenderer.ShoutSpellCast();
            }
            else
            {
                Animator.StartOtherCharacterSpellCast(sourcePlayerID);
                _characterRendererProvider.CharacterRenderers[sourcePlayerID].ShoutSpellCast();
            }

            if (targetPlayerID == _characterRepository.MainCharacter.ID)
            {
                _characterRendererProvider.MainCharacterRenderer.ShowSpellAnimation(spellGraphic);
                _characterRendererProvider.MainCharacterRenderer.ShowDamageCounter(recoveredHP, targetPercentHealth, isHeal: true);
            }
            else
            {
                _characterRendererProvider.CharacterRenderers[targetPlayerID].ShowSpellAnimation(spellGraphic);
                _characterRendererProvider.CharacterRenderers[targetPlayerID].ShowDamageCounter(recoveredHP, targetPercentHealth, isHeal: true);
            }
        }

        public void NotifyEarthquake(byte strength)
        {
            var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
            mapRenderer.StartEarthquake(strength);
        }

        private void ShowWaterSplashiesIfNeeded(CharacterActionState action, ICharacter character, ICharacterRenderer characterRenderer)
        {
            var rp = character.RenderProperties;
            if (action == CharacterActionState.Attacking)
            {
                if (_currentMapProvider.CurrentMap.Tiles[rp.MapY, rp.MapX] == TileSpec.Water)
                    characterRenderer.ShowWaterSplashies();
            }
            else if (action == CharacterActionState.Walking)
            {
                if (_currentMapProvider.CurrentMap.Tiles[rp.GetDestinationY(), rp.GetDestinationX()] == TileSpec.Water)
                    characterRenderer.ShowWaterSplashies();
            }
        }

        private ICharacterAnimator Animator => _hudControlProvider.GetComponent<ICharacterAnimator>(HudControlIdentifier.CharacterAnimator);
    }

    public interface ICharacterAnimationActions
    {
        void Face(EODirection direction);

        void StartWalking();

        bool IsAttacking(int characterId);

        void StartAttacking();
    }
}
