using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Map;
using EndlessClient.Rendering.Metadata;
using EndlessClient.Rendering.Metadata.Models;
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
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
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
        private readonly IPubFileProvider _pubFileProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IMetadataProvider<WeaponMetadata> _weaponMetadataProvider;

        private readonly Random _random;

        public CharacterAnimationActions(IHudControlProvider hudControlProvider,
                                         ICharacterRepository characterRepository,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         ICurrentMapProvider currentMapProvider,
                                         IPubFileProvider pubFileProvider,
                                         IStatusLabelSetter statusLabelSetter,
                                         ISfxPlayer sfxPlayer,
                                         IMetadataProvider<WeaponMetadata> weaponMetadataProvider)
        {
            _hudControlProvider = hudControlProvider;
            _characterRepository = characterRepository;
            _currentMapStateProvider = currentMapStateProvider;
            _characterRendererProvider = characterRendererProvider;
            _currentMapProvider = currentMapProvider;
            _pubFileProvider = pubFileProvider;
            _statusLabelSetter = statusLabelSetter;
            _sfxPlayer = sfxPlayer;
            _weaponMetadataProvider = weaponMetadataProvider;
            _random = new Random();
        }

        public void Face(EODirection direction)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            CancelSpellPrep();
            Animator.MainCharacterFace(direction);
        }

        public void StartWalking(Option<MapCoordinate> targetCoordinate, bool ghosted = false)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            _hudControlProvider.GetComponent<IPeriodicEmoteHandler>(HudControlIdentifier.PeriodicEmoteHandler).CancelArenaBlockTimer();

            CancelSpellPrep();
            Animator.StartMainCharacterWalkAnimation(targetCoordinate, ghosted, () =>
            {
                PlayMainCharacterWalkSfx();
                ShowWaterSplashiesIfNeeded(CharacterActionState.Walking, _characterRepository.MainCharacter.ID);
            });
        }

        public void CancelClickToWalk()
        {
            Animator.CancelClickToWalk();
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

        public void StartOtherCharacterWalkAnimation(int characterID, MapCoordinate destination, EODirection direction)
        {
            if (!_hudControlProvider.IsInGame || !_currentMapStateProvider.Characters.TryGetValue(characterID, out var character))
                return;

            Animator.StartOtherCharacterWalkAnimation(characterID, destination, direction);
            
            ShowWaterSplashiesIfNeeded(CharacterActionState.Walking, characterID);

            if (IsSteppingStone(character.RenderProperties))
                _sfxPlayer.PlaySfx(SoundEffectID.JumpStone);
        }

        public void StartOtherCharacterAttackAnimation(int characterID, int noteIndex = -1)
        {
            if (!_hudControlProvider.IsInGame || !_currentMapStateProvider.Characters.TryGetValue(characterID, out var character))
                return;

            if (noteIndex >= 0 || IsInstrumentWeapon(character.RenderProperties.WeaponGraphic))
                Animator.Emote(characterID, EOLib.Domain.Character.Emote.MusicNotes);

            Animator.StartOtherCharacterAttackAnimation(characterID, () => PlayWeaponSound(character, noteIndex));
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking, characterID);
        }

        public void NotifyWarpLeaveEffect(int characterId, WarpEffect anim)
        {
            if (anim == WarpEffect.Admin && _characterRendererProvider.CharacterRenderers.ContainsKey(characterId))
                _characterRendererProvider.CharacterRenderers[characterId].PlayEffect((int)HardCodedEffect.WarpLeave);
            else if (characterId != _characterRepository.MainCharacter.ID && anim == WarpEffect.Scroll)
                _sfxPlayer.PlaySfx(SoundEffectID.ScrollTeleport);
        }

        public void NotifyWarpEnterEffect(int characterId, WarpEffect anim)
        {
            if (anim == WarpEffect.Admin)
            {
                if (!_characterRendererProvider.CharacterRenderers.ContainsKey(characterId))
                    _characterRendererProvider.NeedsWarpArriveAnimation.Add(characterId);
                else
                    _characterRendererProvider.CharacterRenderers[characterId].PlayEffect((int)HardCodedEffect.WarpArrive);
            }
        }

        public void NotifyPotionEffect(int playerId, int effectId)
        {
            if (playerId == _characterRepository.MainCharacter.ID)
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr => cr.PlayEffect(effectId + 1));
            else if (_characterRendererProvider.CharacterRenderers.ContainsKey(playerId))
                _characterRendererProvider.CharacterRenderers[playerId].PlayEffect(effectId + 1);
        }

        public void NotifyStartSpellCast(int playerId, int spellId)
        {
            var shoutName = _pubFileProvider.ESFFile[spellId].Shout;
            _characterRendererProvider.CharacterRenderers[playerId].ShoutSpellPrep(shoutName.ToLower());
        }

        public void NotifyTargetNpcSpellCast(int playerId)
        {
            // Main player starts its spell cast animation immediately when targeting NPC
            // Other players need to wait for packet to be received and do it here
            // this is some spaghetti...
            if (_characterRendererProvider.CharacterRenderers.ContainsKey(playerId))
            {
                Animator.StartOtherCharacterSpellCast(playerId);
            }
        }

        public void NotifySelfSpellCast(int playerId, int spellId, int spellHp, int percentHealth)
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

        public void NotifyTargetOtherSpellCast(int sourcePlayerID, int targetPlayerID, int spellId, int recoveredHP, int targetPercentHealth)
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

        public void NotifyGroupSpellCast(int playerId, int spellId, int spellHp, List<GroupSpellTarget> spellTargets)
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

        public void NotifyMapEffect(EOLib.IO.Map.MapEffect effect, int strength = 0)
        {
            switch (effect)
            {
                case EOLib.IO.Map.MapEffect.Quake1:
                case EOLib.IO.Map.MapEffect.Quake2:
                case EOLib.IO.Map.MapEffect.Quake3:
                case EOLib.IO.Map.MapEffect.Quake4:
                    var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
                    mapRenderer.StartEarthquake(strength);
                    _sfxPlayer.PlaySfx(SoundEffectID.Earthquake);
                    break;
                case EOLib.IO.Map.MapEffect.HPDrain:
                    _sfxPlayer.PlaySfx(SoundEffectID.MapEffectHPDrain);
                    break;
                case EOLib.IO.Map.MapEffect.TPDrain:
                    _sfxPlayer.PlaySfx(SoundEffectID.MapEffectTPDrain);
                    break;
                case EOLib.IO.Map.MapEffect.Spikes:
                    _sfxPlayer.PlaySfx(SoundEffectID.Spikes);
                    break;
            }
        }

        public void NotifyEmote(int playerId, Emote emote)
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

        public void NotifyEffectAtLocation(MapCoordinate location, int effectId)
        {
            if (_hudControlProvider.IsInGame)
            {
                _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer)
                    .RenderEffect(location, effectId);
            }
        }
        public void NotifyAdminHideEffect(int playerId)
        {
            if (playerId == _characterRepository.MainCharacter.ID)
                _characterRendererProvider.MainCharacterRenderer.MatchSome(cr => cr.PlayEffect((int)HardCodedEffect.AdminHide));
            else if (_characterRendererProvider.CharacterRenderers.ContainsKey(playerId))
                _characterRendererProvider.CharacterRenderers[playerId].PlayEffect((int)HardCodedEffect.AdminHide);
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
            var weaponMetadata = _weaponMetadataProvider.GetValueOrDefault(character.RenderProperties.WeaponGraphic);

            if (noteIndex >= 0 && noteIndex < 36)
            {
                var firstSfx = weaponMetadata.SFX.FirstOrDefault();
                if (firstSfx == SoundEffectID.Harp1)
                {
                    _sfxPlayer.PlayHarpNote(noteIndex);
                }
                else if (firstSfx == SoundEffectID.Guitar1)
                {
                    _sfxPlayer.PlayGuitarNote(noteIndex);
                }
            }
            else
            {
                var index = _random.Next(0, weaponMetadata.SFX.Length);
                _sfxPlayer.PlaySfx(weaponMetadata.SFX[index]);
            }
        }

        private bool IsInstrumentWeapon(int weaponGraphic)
        {
            return Constants.Instruments.Any(x => x == weaponGraphic);
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

        void StartWalking(Option<MapCoordinate> targetCoordinate, bool ghosted = false);

        void CancelClickToWalk();

        void StartAttacking(int noteIndex = -1);

        bool PrepareMainCharacterSpell(int spellId, ISpellTargetable spellTarget);

        void Emote(Emote whichEmote);
    }
}
