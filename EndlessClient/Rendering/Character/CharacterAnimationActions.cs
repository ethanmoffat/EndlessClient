using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Effects;
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
using System.Collections.Generic;
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
        
        private readonly Random _random;

        public CharacterAnimationActions(IHudControlProvider hudControlProvider,
                                         ICharacterRepository characterRepository,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         ICurrentMapProvider currentMapProvider,
                                         ISpikeTrapActions spikeTrapActions,
                                         IPubFileProvider pubFileProvider,
                                         IStatusLabelSetter statusLabelSetter,
                                         ISfxPlayer sfxPlayer)
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

            _random = new Random();
        }

        public void Face(EODirection direction)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            CancelSpellPrep();
            Animator.MainCharacterFace(direction);
        }

        public void StartWalking(Option<MapCoordinate> targetCoordinate)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            CancelSpellPrep();
            Animator.StartMainCharacterWalkAnimation(targetCoordinate, () =>
            {
                PlayMainCharacterWalkSfx();
                ShowWaterSplashiesIfNeeded(CharacterActionState.Walking, _characterRepository.MainCharacter.ID);
            });
        }

        public void StartAttacking(int noteIndex = -1)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            CancelSpellPrep();

            if (noteIndex >= 0 || IsInstrumentWeapon(_characterRepository.MainCharacter.RenderProperties.WeaponGraphic))
                Animator.Emote(_characterRepository.MainCharacter.ID, EOLib.Domain.Character.Emote.MusicNotes);
            Animator.StartMainCharacterAttackAnimation(() => PlayWeaponSound(_characterRepository.MainCharacter, noteIndex));
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking, _characterRepository.MainCharacter.ID);
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

            Animator.StartOtherCharacterAttackAnimation(characterID, () => PlayWeaponSound(_currentMapStateProvider.Characters[characterID], noteIndex));
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking, characterID);
        }

        public void NotifyWarpLeaveEffect(short characterId, WarpAnimation anim)
        {
            if (anim == WarpAnimation.Admin)
                _characterRendererProvider.CharacterRenderers[characterId].PlayEffect((int)HardCodedEffect.WarpLeave);
        }

        public void NotifyWarpEnterEffect(short characterId, WarpAnimation anim)
        {
            if (anim == WarpAnimation.Admin)
            {
                if (!_characterRendererProvider.CharacterRenderers.ContainsKey(characterId))
                    _characterRendererProvider.NeedsWarpArriveAnimation.Add(characterId);
                else
                    _characterRendererProvider.CharacterRenderers[characterId].PlayEffect((int)HardCodedEffect.WarpArrive);
            }
        }

        public void NotifyPotionEffect(short playerId, int effectId)
        {
            if (playerId == _characterRepository.MainCharacter.ID)
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr => cr.PlayEffect(effectId + 1));
            else if (_characterRendererProvider.CharacterRenderers.ContainsKey(playerId))
                _characterRendererProvider.CharacterRenderers[playerId].PlayEffect(effectId + 1);
        }

        public void NotifyStartSpellCast(short playerId, short spellId)
        {
            var shoutName = _pubFileProvider.ESFFile[spellId].Shout;
            _characterRendererProvider.CharacterRenderers[playerId].ShoutSpellPrep(shoutName.ToLower());
        }

        public void NotifyTargetNpcSpellCast(short playerId)
        {
            // Main player starts its spell cast animation immediately when targeting NPC
            // Other players need to wait for packet to be received and do it here
            // this is some spaghetti...
            if (_characterRendererProvider.CharacterRenderers.ContainsKey(playerId))
            {
                Animator.StartOtherCharacterSpellCast(playerId);
            }
        }

        public void NotifySelfSpellCast(short playerId, short spellId, int spellHp, byte percentHealth)
        {
            var spellGraphic = _pubFileProvider.ESFFile[spellId].Graphic;

            if (playerId == _characterRepository.MainCharacter.ID)
            { 
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr =>
                {
                    cr.ShoutSpellCast();
                    cr.PlayEffect(spellGraphic);
                    cr.ShowDamageCounter(spellHp, percentHealth, isHeal: true);
                });
            }
            else
            {
                Animator.StartOtherCharacterSpellCast(playerId);
                _characterRendererProvider.CharacterRenderers[playerId].ShoutSpellCast();
                _characterRendererProvider.CharacterRenderers[playerId].PlayEffect(spellGraphic);
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
                    cr.PlayEffect(spellData.Graphic);
                    cr.ShowDamageCounter(recoveredHP, targetPercentHealth, isHeal: spellData.Type == EOLib.IO.SpellType.Heal);
                });
            }
            else
            {
                _characterRendererProvider.CharacterRenderers[targetPlayerID].PlayEffect(spellData.Graphic);
                _characterRendererProvider.CharacterRenderers[targetPlayerID].ShowDamageCounter(recoveredHP, targetPercentHealth, isHeal: spellData.Type == EOLib.IO.SpellType.Heal);
            }
        }

        public void NotifyGroupSpellCast(short playerId, short spellId, short spellHp, List<GroupSpellTarget> spellTargets)
        {
            if (playerId == _characterRepository.MainCharacter.ID)
            {
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr => cr.ShoutSpellCast());
            }
            else if (_characterRendererProvider.CharacterRenderers.ContainsKey(playerId))
            {
                Animator.StartOtherCharacterSpellCast(playerId);
                _characterRendererProvider.CharacterRenderers[playerId].ShoutSpellCast();
            }
            else
            {
                _currentMapStateProvider.UnknownPlayerIDs.Add(playerId);
            }

            var spellData = _pubFileProvider.ESFFile[spellId];

            foreach (var target in spellTargets)
            {
                if (target.TargetId == _characterRepository.MainCharacter.ID)
                {
                    _characterRendererProvider.MainCharacterRenderer.MatchSome(cr =>
                    {
                        cr.PlayEffect(spellData.Graphic);
                        cr.ShowDamageCounter(spellHp, target.PercentHealth, isHeal: true);
                    });
                }
                else if (_characterRendererProvider.CharacterRenderers.ContainsKey(target.TargetId))
                {
                    _characterRendererProvider.CharacterRenderers[target.TargetId].PlayEffect(spellData.Graphic);
                    _characterRendererProvider.CharacterRenderers[target.TargetId].ShowDamageCounter(spellHp, target.PercentHealth, isHeal: true);
                }
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
            switch (emote)
            {
                case EOLib.Domain.Character.Emote.Trade:
                    _sfxPlayer.PlaySfx(SoundEffectID.TradeAccepted);
                    break;
                case EOLib.Domain.Character.Emote.LevelUp:
                    _sfxPlayer.PlaySfx(SoundEffectID.LevelUp);
                    break;
            }

            try
            {
                Animator.Emote(playerId, emote);
            }
            catch (InvalidOperationException)
            {
                // if still transitioning to in-game state, the game will crash because the in-game control set is not completely set up yet
            }
        }

        public void MakeMainPlayerDrunk()
        {
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_ITEM_USE_DRUNK);
        }

        public void NotifyEffectAtLocation(byte x, byte y, short effectId)
        {
            if (_hudControlProvider.IsInGame)
            {
                _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer)
                    .RenderEffect(x, y, effectId);
            }
        }

        private void ShowWaterSplashiesIfNeeded(CharacterActionState action, int characterID)
        {
            var characterRenderer = characterID == _characterRepository.MainCharacter.ID
                ? _characterRendererProvider.MainCharacterRenderer
                : _characterRendererProvider.CharacterRenderers.ContainsKey(characterID)
                    ? Option.Some(_characterRendererProvider.CharacterRenderers[characterID])
                    : Option.None<ICharacterRenderer>();

            characterRenderer.MatchSome(cr =>
            {
                var rp = cr.Character.RenderProperties;

                if (action == CharacterActionState.Attacking)
                {
                    if (_currentMapProvider.CurrentMap.Tiles[rp.MapY, rp.MapX] == TileSpec.Water)
                    {
                        cr.PlayEffect((int)HardCodedEffect.WaterSplashies);
                        _sfxPlayer.PlaySfx(SoundEffectID.Water);
                    }
                }
                else if (action == CharacterActionState.Walking)
                {
                    if (_currentMapProvider.CurrentMap.Tiles[rp.GetDestinationY(), rp.GetDestinationX()] == TileSpec.Water)
                    {
                        cr.PlayEffect((int)HardCodedEffect.WaterSplashies);
                        _sfxPlayer.PlaySfx(SoundEffectID.Water);
                    }
                }
            });
        }

        private void CancelSpellPrep()
        {
            Animator.MainCharacterCancelSpellPrep();
            _characterRendererProvider.MainCharacterRenderer.MatchSome(r => r.StopShout());
        }

        private void PlayMainCharacterWalkSfx()
        {
            if (_characterRepository.MainCharacter.NoWall)
                _sfxPlayer.PlaySfx(SoundEffectID.NoWallWalk);
            else if (IsSteppingStone(_characterRepository.MainCharacter.RenderProperties))
                _sfxPlayer.PlaySfx(SoundEffectID.JumpStone);
        }

        private void PlayWeaponSound(EOLib.Domain.Character.Character character, int noteIndex = -1)
        {
            if (character.RenderProperties.WeaponGraphic == 0)
            {
                _sfxPlayer.PlaySfx(SoundEffectID.PunchAttack);
                return;
            }

            _pubFileProvider.EIFFile.FirstOrNone(x => x.Type == ItemType.Weapon && x.DollGraphic == character.RenderProperties.WeaponGraphic)
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

        void StartWalking(Option<MapCoordinate> targetCoordinate);

        void StartAttacking(int noteIndex = -1);

        bool PrepareMainCharacterSpell(int spellId, ISpellTargetable spellTarget);

        void Emote(Emote whichEmote);
    }
}
