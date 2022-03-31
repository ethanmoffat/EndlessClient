using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Map;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Optional;

namespace EndlessClient.Rendering.Character
{
    [AutoMappedType]
    public class CharacterAnimationActions : ICharacterAnimationActions, IOtherCharacterAnimationNotifier, IEffectNotifier, IEmoteNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ISpikeTrapActions _spikeTrapActions;
        private readonly IESFFileProvider _esfFileProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;

        public CharacterAnimationActions(IHudControlProvider hudControlProvider,
                                         ICharacterRepository characterRepository,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         ICurrentMapProvider currentMapProvider,
                                         ISpikeTrapActions spikeTrapActions,
                                         IESFFileProvider esfFileProvider,
                                         IStatusLabelSetter statusLabelSetter)
        {
            _hudControlProvider = hudControlProvider;
            _characterRepository = characterRepository;
            _currentMapStateProvider = currentMapStateProvider;
            _characterRendererProvider = characterRendererProvider;
            _currentMapProvider = currentMapProvider;
            _spikeTrapActions = spikeTrapActions;
            _esfFileProvider = esfFileProvider;
            _statusLabelSetter = statusLabelSetter;
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

            Animator.StartMainCharacterWalkAnimation(Option.None<MapCoordinate>());
            ShowWaterSplashiesIfNeeded(CharacterActionState.Walking, _characterRepository.MainCharacter.ID);
        }

        public void StartAttacking()
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartMainCharacterAttackAnimation();
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking, _characterRepository.MainCharacter.ID);
        }

        public void StartOtherCharacterWalkAnimation(int characterID, byte destinationX, byte destinationY, EODirection direction)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartOtherCharacterWalkAnimation(characterID, destinationX, destinationY, direction);
            
            ShowWaterSplashiesIfNeeded(CharacterActionState.Walking, characterID);
            _spikeTrapActions.HideSpikeTrap(characterID);
            _spikeTrapActions.ShowSpikeTrap(characterID);
        }

        public void StartOtherCharacterAttackAnimation(int characterID)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartOtherCharacterAttackAnimation(characterID);
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking, characterID);
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
            if (playerId == _characterRepository.MainCharacter.ID)
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr => cr.ShowPotionAnimation(effectId));
            else if (_characterRendererProvider.CharacterRenderers.ContainsKey(playerId))
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
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr =>
                {
                    cr.ShoutSpellCast();
                    cr.ShowSpellAnimation(spellGraphic);
                    cr.ShowDamageCounter(spellHp, percentHealth, isHeal: true);
                });
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
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr => cr.ShoutSpellCast());
            }
            else
            {
                Animator.StartOtherCharacterSpellCast(sourcePlayerID);
                _characterRendererProvider.CharacterRenderers[sourcePlayerID].ShoutSpellCast();
            }

            if (targetPlayerID == _characterRepository.MainCharacter.ID)
            {
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr =>
                {
                    cr.ShowSpellAnimation(spellGraphic);
                    cr.ShowDamageCounter(recoveredHP, targetPercentHealth, isHeal: true);
                });
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

        public void NotifyEmote(short playerId, Emote emote)
        {
            Animator.Emote(playerId, emote);
        }

        public void MakeMainPlayerDrunk()
        {
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_ITEM_USE_DRUNK);
        }

        private void ShowWaterSplashiesIfNeeded(CharacterActionState action, int characterID)
        {
            var character = characterID == _characterRepository.MainCharacter.ID
                ? Option.Some(_characterRepository.MainCharacter)
                : _currentMapStateProvider.Characters.ContainsKey(characterID)
                    ? Option.Some(_currentMapStateProvider.Characters[characterID])
                    : Option.None<ICharacter>();

            var characterRenderer = characterID == _characterRepository.MainCharacter.ID
                ? _characterRendererProvider.MainCharacterRenderer
                : _characterRendererProvider.CharacterRenderers.ContainsKey(characterID)
                    ? Option.Some(_characterRendererProvider.CharacterRenderers[characterID])
                    : Option.None<ICharacterRenderer>();

            character.MatchSome(c =>
            {
                var rp = c.RenderProperties;

                characterRenderer.MatchSome(cr =>
                {
                    if (action == CharacterActionState.Attacking)
                    {
                        if (_currentMapProvider.CurrentMap.Tiles[rp.MapY, rp.MapX] == TileSpec.Water)
                            cr.ShowWaterSplashies();
                    }
                    else if (action == CharacterActionState.Walking)
                    {
                        if (_currentMapProvider.CurrentMap.Tiles[rp.GetDestinationY(), rp.GetDestinationX()] == TileSpec.Water)
                            cr.ShowWaterSplashies();
                    }

                });
            });
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
