using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Map;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Spells;
using EOLib.IO;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Optional;
using Optional.Collections;
using System;
using System.Linq;

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
        private readonly IPubFileProvider _pubFileProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IConfigurationProvider _configurationProvider;
        
        private readonly Random _random;

        public CharacterAnimationActions(IHudControlProvider hudControlProvider,
                                         ICharacterRepository characterRepository,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         ICurrentMapProvider currentMapProvider,
                                         ISpikeTrapActions spikeTrapActions,
                                         IPubFileProvider pubFileProvider,
                                         IStatusLabelSetter statusLabelSetter,
                                         ISfxPlayer sfxPlayer,
                                         IConfigurationProvider configurationProvider)
        {
            _hudControlProvider = hudControlProvider;
            _characterRepository = characterRepository;
            _currentMapStateProvider = currentMapStateProvider;
            _characterRendererProvider = characterRendererProvider;
            _currentMapProvider = currentMapProvider;
            _spikeTrapActions = spikeTrapActions;
            _pubFileProvider = pubFileProvider;
            _statusLabelSetter = statusLabelSetter;
            _sfxPlayer = sfxPlayer;
            _configurationProvider = configurationProvider;

            _random = new Random();
        }

        public void Face(EODirection direction)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            CancelSpellPrep();
            Animator.MainCharacterFace(direction);
        }

        public void StartWalking()
        {
            if (!_hudControlProvider.IsInGame)
                return;

            CancelSpellPrep();
            Animator.StartMainCharacterWalkAnimation(Option.None<MapCoordinate>());
            ShowWaterSplashiesIfNeeded(CharacterActionState.Walking, _characterRepository.MainCharacter.ID);

            if (_characterRepository.MainCharacter.NoWall)
                _sfxPlayer.PlaySfx(SoundEffectID.NoWallWalk);
            else if (IsSteppingStone(_characterRepository.MainCharacter.RenderProperties))
                _sfxPlayer.PlaySfx(SoundEffectID.JumpStone);
        }

        public void StartAttacking(int noteIndex = -1)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            CancelSpellPrep();

            if (noteIndex >= 0 || IsInstrumentWeapon(_characterRepository.MainCharacter.RenderProperties.WeaponGraphic))
                Animator.Emote(_characterRepository.MainCharacter.ID, EOLib.Domain.Character.Emote.MusicNotes);
            Animator.StartMainCharacterAttackAnimation();
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking, _characterRepository.MainCharacter.ID);

            PlayWeaponSound(_characterRepository.MainCharacter, noteIndex);
        }

        public bool PrepareMainCharacterSpell(int spellId, ISpellTargetable spellTarget)
        {
            if (!_hudControlProvider.IsInGame)
                return false;

            var spellData = _pubFileProvider.ESFFile[spellId];
            _characterRendererProvider.MainCharacterRenderer.MatchSome(r => r.ShoutSpellPrep(spellData.Shout));
            return Animator.MainCharacterShoutSpellPrep(spellData, spellTarget);
        }

        public void Emote(Emote whichEmote)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.Emote(_characterRepository.MainCharacter.ID, whichEmote);
        }

        public void StartOtherCharacterWalkAnimation(int characterID, byte destinationX, byte destinationY, EODirection direction)
        {
            if (!_hudControlProvider.IsInGame || !_currentMapStateProvider.Characters.ContainsKey(characterID))
                return;

            Animator.StartOtherCharacterWalkAnimation(characterID, destinationX, destinationY, direction);
            
            ShowWaterSplashiesIfNeeded(CharacterActionState.Walking, characterID);
            _spikeTrapActions.HideSpikeTrap(characterID);
            _spikeTrapActions.ShowSpikeTrap(characterID);

            if (IsSteppingStone(_currentMapStateProvider.Characters[characterID].RenderProperties))
                _sfxPlayer.PlaySfx(SoundEffectID.JumpStone);
        }

        public void StartOtherCharacterAttackAnimation(int characterID, int noteIndex = -1)
        {
            if (!_hudControlProvider.IsInGame || !_currentMapStateProvider.Characters.ContainsKey(characterID))
                return;

            if (noteIndex >= 0 || IsInstrumentWeapon(_currentMapStateProvider.Characters[characterID].RenderProperties.WeaponGraphic))
                Animator.Emote(characterID, EOLib.Domain.Character.Emote.MusicNotes);

            Animator.StartOtherCharacterAttackAnimation(characterID);
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking, characterID);

            PlayWeaponSound(_currentMapStateProvider.Characters[characterID], noteIndex);
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
            var shoutName = _pubFileProvider.ESFFile[spellId].Shout;
            _characterRendererProvider.CharacterRenderers[playerId].ShoutSpellPrep(shoutName.ToLower());
        }

        public void NotifySelfSpellCast(short playerId, short spellId, int spellHp, byte percentHealth)
        {
            var spellGraphic = _pubFileProvider.ESFFile[spellId].Graphic;

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
            if (sourcePlayerID == _characterRepository.MainCharacter.ID)
            {
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr => cr.ShoutSpellCast());
            }
            else
            {
                Animator.StartOtherCharacterSpellCast(sourcePlayerID);
                _characterRendererProvider.CharacterRenderers[sourcePlayerID].ShoutSpellCast();
            }

            var spellData = _pubFileProvider.ESFFile[spellId];

            if (targetPlayerID == _characterRepository.MainCharacter.ID)
            {
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr =>
                {
                    cr.ShowSpellAnimation(spellData.Graphic);
                    cr.ShowDamageCounter(recoveredHP, targetPercentHealth, isHeal: spellData.Type == EOLib.IO.SpellType.Heal);
                });
            }
            else
            {
                _characterRendererProvider.CharacterRenderers[targetPlayerID].ShowSpellAnimation(spellData.Graphic);
                _characterRendererProvider.CharacterRenderers[targetPlayerID].ShowDamageCounter(recoveredHP, targetPercentHealth, isHeal: spellData.Type == EOLib.IO.SpellType.Heal);
            }
        }

        public void NotifyMapEffect(MapEffect effect, byte strength = 0)
        {
            switch (effect)
            {
                case MapEffect.Quake1:
                case MapEffect.Quake2:
                case MapEffect.Quake3:
                case MapEffect.Quake4:
                    var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
                    mapRenderer.StartEarthquake(strength);
                    _sfxPlayer.PlaySfx(SoundEffectID.Earthquake);
                    break;
                case MapEffect.HPDrain:
                    _sfxPlayer.PlaySfx(SoundEffectID.MapEffectHPDrain);
                    break;
                case MapEffect.TPDrain:
                    _sfxPlayer.PlaySfx(SoundEffectID.MapEffectTPDrain);
                    break;
                case MapEffect.Spikes:
                    _sfxPlayer.PlaySfx(SoundEffectID.Spikes);
                    break;
            }
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
                    : Option.None<EOLib.Domain.Character.Character>();

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

        private void CancelSpellPrep()
        {
            Animator.MainCharacterCancelSpellPrep();
            _characterRendererProvider.MainCharacterRenderer.MatchSome(r => r.StopShout());
        }

        private void PlayWeaponSound(EOLib.Domain.Character.Character character, int noteIndex = -1)
        {
            if (!_configurationProvider.SoundEnabled)
                return;

            if (character.RenderProperties.WeaponGraphic == 0)
            {
                _sfxPlayer.PlaySfx(SoundEffectID.PunchAttack);
                return;
            }

            _pubFileProvider.EIFFile.SingleOrNone(x => x.Type == ItemType.Weapon && x.DollGraphic == character.RenderProperties.WeaponGraphic)
                .MatchSome(x =>
                {
                    var instrumentIndex = Constants.InstrumentIDs.ToList().FindIndex(y => y == x.ID);
                    switch (instrumentIndex)
                    {
                        case 0:
                            {
                                if (noteIndex < 0 || noteIndex >= 36)
                                    _sfxPlayer.PlaySfx(SoundEffectID.Harp1 + _random.Next(0, 3));
                                else
                                    _sfxPlayer.PlayHarpNote(noteIndex);
                            }
                            break;
                        case 1:
                            {
                                if (noteIndex < 0 || noteIndex >= 36)
                                    _sfxPlayer.PlaySfx(SoundEffectID.Guitar1 + _random.Next(0, 3));
                                else
                                    _sfxPlayer.PlayGuitarNote(noteIndex);
                            }
                            break;
                        default:
                            switch (x.SubType)
                            {
                                case ItemSubType.Ranged:
                                    if (x.ID == 365 && string.Equals(x.Name, "gun", System.StringComparison.OrdinalIgnoreCase))
                                        _sfxPlayer.PlaySfx(SoundEffectID.Gun);
                                    else
                                        _sfxPlayer.PlaySfx(SoundEffectID.AttackBow);
                                    break;
                                default:
                                    _sfxPlayer.PlaySfx(SoundEffectID.MeleeWeaponAttack);
                                    break;
                            }
                            break;
                    }
                });
        }

        private bool IsInstrumentWeapon(int weaponGraphic)
        {
            return _pubFileProvider.EIFFile
                .SingleOrNone(x => x.Type == ItemType.Weapon && x.DollGraphic == weaponGraphic)
                .Match(some: x => Constants.InstrumentIDs.ToList().FindIndex(y => y == x.ID) >= 0, none: () => false);
        }

        private bool IsSteppingStone(CharacterRenderProperties renderProps)
        {
            return _currentMapProvider.CurrentMap.Tiles[renderProps.MapY, renderProps.MapX] == TileSpec.Jump
                || _currentMapProvider.CurrentMap.Tiles[renderProps.GetDestinationY(), renderProps.GetDestinationX()] == TileSpec.Jump;
        }

        private ICharacterAnimator Animator => _hudControlProvider.GetComponent<ICharacterAnimator>(HudControlIdentifier.CharacterAnimator);
    }

    public interface ICharacterAnimationActions
    {
        void Face(EODirection direction);

        void StartWalking();

        void StartAttacking(int noteIndex = -1);

        bool PrepareMainCharacterSpell(int spellId, ISpellTargetable spellTarget);

        void Emote(Emote whichEmote);
    }
}
