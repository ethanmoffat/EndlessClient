using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Spells;
using EndlessClient.Rendering.Metadata;
using EndlessClient.Rendering.Metadata.Models;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Spells;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Optional;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EndlessClient.Rendering.Character
{
    public class CharacterAnimator : GameComponent, ICharacterAnimator
    {
        public const int TICKS_PER_WALK_FRAME = 9; // 9 x10ms ticks per walk frame
        public const int TICKS_PER_FRAME = 12; // 12 x10ms ticks per attack frame
        public const int TICKS_PER_CAST_TIME = 48;

        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ISpellSlotDataRepository _spellSlotDataRepository;
        private readonly ICharacterActions _characterActions;
        private readonly IWalkValidationActions _walkValidationActions;
        private readonly IPathFinder _pathFinder;
        private readonly IFixedTimeStepRepository _fixedTimeStepRepository;
        private readonly IMetadataProvider<WeaponMetadata> _weaponMetadataProvider;

        // todo: this state should really be managed better
        private readonly Dictionary<int, EODirection> _queuedDirections;
        private readonly Dictionary<int, MapCoordinate> _queuedPositions;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartWalkingTimes;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartAttackingTimes;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartSpellCastTimes;
        private readonly Dictionary<int, RenderFrameActionTime> _startEmoteTimes;

        private Option<ulong> _mainPlayerStartShoutTick;
        private ESFRecord _shoutSpellData;
        private ISpellTargetable _spellTarget;

        private Queue<MapCoordinate> _walkPath;
        private Option<MapCoordinate> _targetCoordinate;

        public CharacterAnimator(IEndlessGameProvider gameProvider,
                                 ICharacterRepository characterRepository,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 ICurrentMapProvider currentMapProvider,
                                 ISpellSlotDataRepository spellSlotDataRepository,
                                 ICharacterActions characterActions,
                                 IWalkValidationActions walkValidationActions,
                                 IPathFinder pathFinder,
                                 IFixedTimeStepRepository fixedTimeStepRepository,
                                 IMetadataProvider<WeaponMetadata> weaponMetadataProvider)
            : base((Game)gameProvider.Game)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _currentMapProvider = currentMapProvider;
            _spellSlotDataRepository = spellSlotDataRepository;
            _characterActions = characterActions;
            _walkValidationActions = walkValidationActions;
            _pathFinder = pathFinder;
            _fixedTimeStepRepository = fixedTimeStepRepository;
            _weaponMetadataProvider = weaponMetadataProvider;
            _queuedDirections = new Dictionary<int, EODirection>();
            _queuedPositions = new Dictionary<int, MapCoordinate>();
            _otherPlayerStartWalkingTimes = new Dictionary<int, RenderFrameActionTime>();
            _otherPlayerStartAttackingTimes = new Dictionary<int, RenderFrameActionTime>();
            _otherPlayerStartSpellCastTimes = new Dictionary<int, RenderFrameActionTime>();
            _startEmoteTimes = new Dictionary<int, RenderFrameActionTime>();

            _walkPath = new Queue<MapCoordinate>();
        }

        public override void Update(GameTime gameTime)
        {
            if (_fixedTimeStepRepository.IsWalkUpdateFrame)
            {
                AnimateCharacterWalking();
            }

            AnimateCharacterAttacking();
            AnimateCharacterSpells();
            AnimateCharacterEmotes();

            base.Update(gameTime);
        }

        public void MainCharacterFace(EODirection direction)
        {
            if (_otherPlayerStartWalkingTimes.ContainsKey(_characterRepository.MainCharacter.ID))
            {
                _queuedDirections[_characterRepository.MainCharacter.ID] = direction;
                return;
            }

            var renderProperties = _characterRepository.MainCharacter.RenderProperties.WithDirection(direction);
            var newMainCharacter = _characterRepository.MainCharacter.WithRenderProperties(renderProperties);
            _characterRepository.MainCharacter = newMainCharacter;

            _characterActions.Face(direction);
        }

        public void StartMainCharacterWalkAnimation(Option<MapCoordinate> targetCoordinate, bool ghosted, Action sfxCallback)
        {
            _walkPath.Clear();

            targetCoordinate.Match(
                some: tc =>
                {
                    _targetCoordinate = targetCoordinate;

                    var rp = _characterRepository.MainCharacter.RenderProperties;
                    var characterCoord = new MapCoordinate(rp.MapX, rp.MapY);

                    _walkPath = _pathFinder.FindPath(characterCoord, tc);

                    if (!_walkPath.Any()) return;

                    if (!_otherPlayerStartWalkingTimes.ContainsKey(_characterRepository.MainCharacter.ID))
                    {
                        rp = FaceTarget(characterCoord, _walkPath.Peek(), rp);
                        _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(rp);
                    }

                    doTheWalk();
                },
                none: doTheWalk);

            void doTheWalk()
            {
                if (_otherPlayerStartWalkingTimes.ContainsKey(_characterRepository.MainCharacter.ID))
                {
                    _otherPlayerStartWalkingTimes[_characterRepository.MainCharacter.ID].SetReplay(sfxCallback);
                    return;
                }

                var startWalkingTime = new RenderFrameActionTime(_characterRepository.MainCharacter.ID, _fixedTimeStepRepository.TickCount, sfxCallback);
                _otherPlayerStartWalkingTimes.Add(_characterRepository.MainCharacter.ID, startWalkingTime);

                _characterActions.Walk(ghosted);
                startWalkingTime.SoundEffect();
            }
        }

        public void CancelClickToWalk()
        {
            _walkPath.Clear();
        }

        public void StartMainCharacterAttackAnimation(Action sfxCallback)
        {
            if (_otherPlayerStartAttackingTimes.ContainsKey(_characterRepository.MainCharacter.ID))
            {
                _otherPlayerStartAttackingTimes[_characterRepository.MainCharacter.ID].SetReplay(sfxCallback);
                return;
            }

            var startAttackingTime = new RenderFrameActionTime(_characterRepository.MainCharacter.ID, _fixedTimeStepRepository.TickCount, sfxCallback);
            _otherPlayerStartAttackingTimes.Add(_characterRepository.MainCharacter.ID, startAttackingTime);
        }

        public bool MainCharacterShoutSpellPrep(ESFRecord spellData, ISpellTargetable target)
        {
            if (_mainPlayerStartShoutTick.HasValue)
                return false;

            _mainPlayerStartShoutTick = Option.Some(_fixedTimeStepRepository.TickCount);
            _shoutSpellData = spellData;
            _spellTarget = target;
            return true;
        }

        public void MainCharacterCancelSpellPrep()
        {
            _mainPlayerStartShoutTick = Option.None<ulong>();
            _shoutSpellData = null;
            _spellTarget = null;

            _spellSlotDataRepository.SelectedSpellSlot = Option.None<int>();
            _spellSlotDataRepository.SpellIsPrepared = false;
        }

        public void StartOtherCharacterWalkAnimation(int characterID, MapCoordinate destination, EODirection direction)
        {
            if (_otherPlayerStartWalkingTimes.ContainsKey(characterID))
            {
                _otherPlayerStartWalkingTimes[characterID].SetReplay();
                _queuedDirections[characterID] = direction;
                _queuedPositions[characterID] = destination;
                return;
            }

            var startWalkingTimeAndID = new RenderFrameActionTime(characterID, _fixedTimeStepRepository.TickCount);
            _otherPlayerStartWalkingTimes.Add(characterID, startWalkingTimeAndID);
        }

        public void StartOtherCharacterAttackAnimation(int characterID, Action sfxCallback)
        {
            if (_otherPlayerStartAttackingTimes.ContainsKey(characterID))
            {
                _otherPlayerStartAttackingTimes[characterID].SetReplay(sfxCallback);
                return;
            }

            var startAttackingTime = new RenderFrameActionTime(characterID, _fixedTimeStepRepository.TickCount, sfxCallback);
            _otherPlayerStartAttackingTimes.Add(characterID, startAttackingTime);
        }

        public void StartOtherCharacterSpellCast(int characterID)
        {
            if (_otherPlayerStartWalkingTimes.ContainsKey(characterID) ||
                _otherPlayerStartAttackingTimes.ContainsKey(characterID))
                return;

            if (_otherPlayerStartSpellCastTimes.ContainsKey(characterID))
            {
                ResetCharacterAnimationFrames(characterID);
                _otherPlayerStartSpellCastTimes.Remove(characterID);
            }

            var startAttackingTimeAndID = new RenderFrameActionTime(characterID, _fixedTimeStepRepository.TickCount);
            _otherPlayerStartSpellCastTimes.Add(characterID, startAttackingTimeAndID);
        }

        public bool Emote(int characterID, Emote whichEmote)
        {
            if (((_otherPlayerStartWalkingTimes.ContainsKey(characterID) ||
                _otherPlayerStartAttackingTimes.ContainsKey(characterID) ||
                _otherPlayerStartSpellCastTimes.ContainsKey(characterID)) && whichEmote != EOLib.Domain.Character.Emote.LevelUp) ||
                _startEmoteTimes.ContainsKey(characterID))
                return false;

            var startEmoteTime = new RenderFrameActionTime(characterID, _fixedTimeStepRepository.TickCount);
            if (characterID == _characterRepository.MainCharacter.ID)
            {
                var rp = _characterRepository.MainCharacter.RenderProperties.WithEmote(whichEmote);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(rp);
            }
            else if (_currentMapStateRepository.Characters.TryGetValue(characterID, out var otherCharacter))
            {
                var rp = otherCharacter.RenderProperties.WithEmote(whichEmote);
                _currentMapStateRepository.Characters.Update(otherCharacter, otherCharacter.WithRenderProperties(rp));
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(characterID);
                return false;
            }

            _startEmoteTimes[characterID] = startEmoteTime;
            return true;
        }

        public void StopAllCharacterAnimations()
        {
            _otherPlayerStartWalkingTimes.Clear();
            _otherPlayerStartAttackingTimes.Clear();
            _otherPlayerStartSpellCastTimes.Clear();
            _walkPath.Clear();

            _characterRepository.MainCharacter =
                _characterRepository.MainCharacter.WithRenderProperties(
                    _characterRepository.MainCharacter.RenderProperties.ResetAnimationFrames());

            var characterPairs = _currentMapStateRepository.Characters
                .Select(c => (Old: c, New: c.WithRenderProperties(c.RenderProperties.ResetAnimationFrames())))
                .ToList();
            foreach (var (Old, New) in characterPairs)
                _currentMapStateRepository.Characters.Update(Old, New);
        }

        #region Walk Animation

        private void AnimateCharacterWalking()
        {
            var playersDoneWalking = new List<int>();
            foreach (var pair in _otherPlayerStartWalkingTimes.Values)
            {
                var sendWalk = false;

                if ((_fixedTimeStepRepository.TickCount - pair.ActionTick) >= TICKS_PER_WALK_FRAME)
                {
                    GetCurrentCharacterFromRepository(pair).Match(
                        none: () => playersDoneWalking.Add(pair.UniqueID),
                        some: currentCharacter =>
                        {
                            var renderProperties = currentCharacter.RenderProperties;
                            var nextFrameRenderProperties = AnimateOneWalkFrame(renderProperties);

                            pair.UpdateActionStartTime(_fixedTimeStepRepository.TickCount);
                            if (nextFrameRenderProperties.IsActing(CharacterActionState.Standing))
                            {
                                var isMainCharacter = currentCharacter == _characterRepository.MainCharacter;

                                if (pair.Replay)
                                {
                                    var nextFramePropertiesWithDirection = _queuedDirections.ContainsKey(pair.UniqueID)
                                        ? nextFrameRenderProperties.WithDirection(_queuedDirections[pair.UniqueID])
                                        : nextFrameRenderProperties;
                                    _queuedDirections.Remove(pair.UniqueID);

                                    var canMoveToDestinationCoordinates = _walkValidationActions.CanMoveToCoordinates(
                                        nextFramePropertiesWithDirection.GetDestinationX(),
                                        nextFramePropertiesWithDirection.GetDestinationY()) == WalkValidationResult.Walkable;

                                    if (!isMainCharacter || (isMainCharacter && canMoveToDestinationCoordinates))
                                    {
                                        // send the walk packet after the game state has been updated so the correct coordinates are sent
                                        sendWalk = isMainCharacter;

                                        var extraFrameProps = AnimateOneWalkFrame(nextFramePropertiesWithDirection.ResetAnimationFrames());
                                        pair.ClearReplay();

                                        nextFrameRenderProperties = extraFrameProps;
                                        pair.SoundEffect();
                                    }
                                    else
                                    {
                                        // tried to replay but the new destination position is not walkable
                                        playersDoneWalking.Add(pair.UniqueID);
                                    }
                                }
                                else if (isMainCharacter && _walkPath.Any())
                                {
                                    var characterCoord = new MapCoordinate(nextFrameRenderProperties.MapX, nextFrameRenderProperties.MapY);

                                    _walkPath = _targetCoordinate.Match(
                                        some: tc =>
                                        {
                                            if (tc.Equals(characterCoord))
                                                return new Queue<MapCoordinate>();
                                            return _pathFinder.FindPath(characterCoord, tc);
                                        },
                                        none: () => new Queue<MapCoordinate>());

                                    if (_walkPath.Any())
                                    {
                                        var next = _walkPath.Dequeue();
                                        nextFrameRenderProperties = FaceTarget(characterCoord, next, nextFrameRenderProperties);

                                        sendWalk = true;
                                        pair.SoundEffect();
                                        nextFrameRenderProperties = AnimateOneWalkFrame(nextFrameRenderProperties.ResetAnimationFrames());
                                    }
                                    else
                                    {
                                        playersDoneWalking.Add(pair.UniqueID);
                                    }
                                }
                                else
                                {
                                    if (_queuedPositions.ContainsKey(pair.UniqueID))
                                    {
                                        nextFrameRenderProperties = nextFrameRenderProperties
                                            .WithMapX(_queuedPositions[pair.UniqueID].X)
                                            .WithMapY(_queuedPositions[pair.UniqueID].Y);
                                        _queuedPositions.Remove(pair.UniqueID);
                                    }

                                    playersDoneWalking.Add(pair.UniqueID);
                                }
                            }

                            var nextFrameCharacter = currentCharacter.WithRenderProperties(nextFrameRenderProperties);
                            UpdateCharacterInRepository(currentCharacter, nextFrameCharacter);

                            if (sendWalk)
                            {
                                _characterActions.Walk(false);
                            }
                        });
                }
            }

            foreach (var key in playersDoneWalking)
                _otherPlayerStartWalkingTimes.Remove(key);
        }

        private CharacterRenderProperties AnimateOneWalkFrame(CharacterRenderProperties renderProperties)
        {
            var isSteppingStone = _currentMapProvider.CurrentMap.Tiles[renderProperties.MapY, renderProperties.MapX] == TileSpec.Jump
                || _currentMapProvider.CurrentMap.Tiles[renderProperties.GetDestinationY(), renderProperties.GetDestinationX()] == TileSpec.Jump;

            var nextFrameRenderProperties = renderProperties.WithNextWalkFrame(isSteppingStone);
            if (nextFrameRenderProperties.CurrentAction != CharacterActionState.Walking)
            {
                nextFrameRenderProperties = nextFrameRenderProperties
                    .WithMapX(nextFrameRenderProperties.GetDestinationX())
                    .WithMapY(nextFrameRenderProperties.GetDestinationY());
            }

            return nextFrameRenderProperties;
        }

        private CharacterRenderProperties FaceTarget(MapCoordinate characterCoord, MapCoordinate next, CharacterRenderProperties rp)
        {
            var diff = next - characterCoord;

            if (diff.X != 0 && diff.Y != 0)
                throw new InvalidOperationException("Trying to move in a diagonal.");

            if (diff.X < 0)
            {
                return rp.WithDirection(EODirection.Left);
            }
            else if (diff.X > 0)
            {
                return rp.WithDirection(EODirection.Right);
            }
            else if (diff.Y < 0)
            {
                return rp.WithDirection(EODirection.Up);
            }
            else if (diff.Y > 0)
            {
                return rp.WithDirection(EODirection.Down);
            }

            return rp;
        }

        #endregion

        #region Attack Animation

        private void AnimateCharacterAttacking()
        {
            var playersDoneAttacking = new HashSet<int>();
            foreach (var pair in _otherPlayerStartAttackingTimes.Values)
            {
                if ((_fixedTimeStepRepository.TickCount - pair.ActionTick) >= TICKS_PER_FRAME)
                {
                    GetCurrentCharacterFromRepository(pair).Match(
                        none: () => playersDoneAttacking.Add(pair.UniqueID),
                        some: currentCharacter =>
                        {
                            var renderProperties = currentCharacter.RenderProperties;
                            var isRanged = _weaponMetadataProvider.GetValueOrDefault(renderProperties.WeaponGraphic).Ranged;
                            var nextFrameRenderProperties = renderProperties.WithNextAttackFrame(isRanged);

                            if (nextFrameRenderProperties.ActualAttackFrame == 2)
                                pair.SoundEffect();

                            pair.UpdateActionStartTime(_fixedTimeStepRepository.TickCount);
                            if (nextFrameRenderProperties.IsActing(CharacterActionState.Standing))
                            {
                                if (pair.Replay)
                                {
                                    nextFrameRenderProperties = renderProperties.ResetAnimationFrames().WithNextAttackFrame(isRanged);
                                    pair.ClearReplay();
                                }
                                else
                                {
                                    playersDoneAttacking.Add(pair.UniqueID);
                                }
                            }

                            var nextFrameCharacter = currentCharacter.WithRenderProperties(nextFrameRenderProperties);
                            UpdateCharacterInRepository(currentCharacter, nextFrameCharacter);
                        });
                }
            }

            foreach (var key in playersDoneAttacking)
                _otherPlayerStartAttackingTimes.Remove(key);
        }

        #endregion

        #region Spell Animation

        private void AnimateCharacterSpells()
        {
            _mainPlayerStartShoutTick.MatchSome(startTick =>
            {
                if (_fixedTimeStepRepository.TickCount - startTick >= (ulong)(_shoutSpellData.CastTime * TICKS_PER_CAST_TIME))
                {
                    _otherPlayerStartSpellCastTimes.Add(_characterRepository.MainCharacter.ID, new RenderFrameActionTime(_characterRepository.MainCharacter.ID, _fixedTimeStepRepository.TickCount));
                    _characterActions.CastSpell(_shoutSpellData.ID, _spellTarget);
                    MainCharacterCancelSpellPrep();

                    var nextRenderProps = _characterRepository.MainCharacter.RenderProperties.WithCurrentAction(CharacterActionState.SpellCast);
                    _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(nextRenderProps);
                }
            });

            var playersDoneCasting = new HashSet<int>();
            foreach (var pair in _otherPlayerStartSpellCastTimes.Values)
            {
                if ((_fixedTimeStepRepository.TickCount - pair.ActionTick) >= TICKS_PER_FRAME)
                {
                    GetCurrentCharacterFromRepository(pair).Match(
                        none: () => playersDoneCasting.Add(pair.UniqueID),
                        some: currentCharacter =>
                        {
                            var renderProperties = currentCharacter.RenderProperties;
                            var nextFrameRenderProperties = renderProperties.WithNextSpellCastFrame();

                            pair.UpdateActionStartTime(_fixedTimeStepRepository.TickCount);
                            if (nextFrameRenderProperties.IsActing(CharacterActionState.Standing))
                                playersDoneCasting.Add(pair.UniqueID);

                            var nextFrameCharacter = currentCharacter.WithRenderProperties(nextFrameRenderProperties);
                            UpdateCharacterInRepository(currentCharacter, nextFrameCharacter);
                        });
                }
            }

            foreach (var key in playersDoneCasting)
                _otherPlayerStartSpellCastTimes.Remove(key);
        }

        #endregion

        #region Emote Animation

        private void AnimateCharacterEmotes()
        {
            var playersDoneEmoting = new HashSet<int>();
            foreach (var pair in _startEmoteTimes.Values)
            {
                if ((_fixedTimeStepRepository.TickCount - pair.ActionTick) >= TICKS_PER_FRAME * 2)
                {
                    GetCurrentCharacterFromRepository(pair).Match(
                        none: () => playersDoneEmoting.Add(pair.UniqueID),
                        some: currentCharacter =>
                        {
                            var renderProperties = currentCharacter.RenderProperties;
                            var nextFrameRenderProperties = renderProperties.WithNextEmoteFrame();

                            pair.UpdateActionStartTime(_fixedTimeStepRepository.TickCount);
                            if (nextFrameRenderProperties.IsActing(CharacterActionState.Standing, CharacterActionState.Sitting))
                                playersDoneEmoting.Add(pair.UniqueID);

                            var nextFrameCharacter = currentCharacter.WithRenderProperties(nextFrameRenderProperties);
                            UpdateCharacterInRepository(currentCharacter, nextFrameCharacter);
                        });
                }
            }

            foreach (var key in playersDoneEmoting)
                _startEmoteTimes.Remove(key);
        }

        #endregion

        private Option<EOLib.Domain.Character.Character> GetCurrentCharacterFromRepository(RenderFrameActionTime pair)
        {
            return pair.UniqueID == _characterRepository.MainCharacter.ID
                ? Option.Some(_characterRepository.MainCharacter)
                : _currentMapStateRepository.Characters.TryGetValue(pair.UniqueID, out var character)
                    ? Option.Some(character)
                    : Option.None<EOLib.Domain.Character.Character>();
        }

        private void UpdateCharacterInRepository(EOLib.Domain.Character.Character currentCharacter,
                                                 EOLib.Domain.Character.Character nextFrameCharacter)
        {
            if (currentCharacter == _characterRepository.MainCharacter)
            {
                _characterRepository.MainCharacter = nextFrameCharacter;
            }
            else
            {
                _currentMapStateRepository.Characters.Update(currentCharacter, nextFrameCharacter);
            }
        }

        private void ResetCharacterAnimationFrames(int characterID)
        {
            var character = _currentMapStateRepository.Characters[characterID];
            var renderProps = character.RenderProperties.ResetAnimationFrames();
            var newCharacter = character.WithRenderProperties(renderProps);
            _currentMapStateRepository.Characters.Update(character, newCharacter);
        }
    }

    public interface ICharacterAnimator : IGameComponent
    {
        void MainCharacterFace(EODirection direction);

        void StartMainCharacterWalkAnimation(Option<MapCoordinate> targetCoordinate, bool ghosted, Action sfxCallback);

        void CancelClickToWalk();

        void StartMainCharacterAttackAnimation(Action sfxCallback);

        bool MainCharacterShoutSpellPrep(ESFRecord spellData, ISpellTargetable spellTarget);

        void MainCharacterCancelSpellPrep();

        void StartOtherCharacterWalkAnimation(int characterID, MapCoordinate destination, EODirection direction);

        void StartOtherCharacterAttackAnimation(int characterID, Action sfxCallback);

        void StartOtherCharacterSpellCast(int characterID);

        bool Emote(int characterID, Emote whichEmote);

        void StopAllCharacterAnimations();
    }
}