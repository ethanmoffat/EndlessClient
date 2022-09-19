using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Spells;
using EndlessClient.Input;
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
        public const int WALK_FRAME_TIME_MS = 120;
        public const int ATTACK_FRAME_TIME_MS = 100;
        public const int EMOTE_FRAME_TIME_MS = 250;

        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ISpellSlotDataRepository _spellSlotDataRepository;
        private readonly IUserInputRepository _userInputRepository;
        private readonly ICharacterActions _characterActions;
        private readonly IWalkValidationActions _walkValidationActions;
        private readonly IPathFinder _pathFinder;

        // todo: this state should really be managed better
        private readonly Dictionary<int, EODirection> _queuedDirections;
        private readonly Dictionary<int, MapCoordinate> _queuedPositions;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartWalkingTimes;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartAttackingTimes;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartSpellCastTimes;
        private readonly Dictionary<int, RenderFrameActionTime> _startEmoteTimes;

        private Option<Stopwatch> _mainPlayerStartShoutTime;
        private ESFRecord _shoutSpellData;
        private ISpellTargetable _spellTarget;

        private Queue<MapCoordinate> _walkPath;
        private Option<MapCoordinate> _targetCoordinate;

        private bool _clickHandled;

        public CharacterAnimator(IEndlessGameProvider gameProvider,
                                 ICharacterRepository characterRepository,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 ICurrentMapProvider currentMapProvider,
                                 ISpellSlotDataRepository spellSlotDataRepository,
                                 IUserInputRepository userInputRepository,
                                 ICharacterActions characterActions,
                                 IWalkValidationActions walkValidationActions,
                                 IPathFinder pathFinder)
            : base((Game) gameProvider.Game)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _currentMapProvider = currentMapProvider;
            _spellSlotDataRepository = spellSlotDataRepository;
            _userInputRepository = userInputRepository;
            _characterActions = characterActions;
            _walkValidationActions = walkValidationActions;
            _pathFinder = pathFinder;
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
            if (_userInputRepository.ClickHandled && !_userInputRepository.WalkClickHandled && _walkPath.Any())
                _clickHandled = true;

            AnimateCharacterWalking();
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

        public void StartMainCharacterWalkAnimation(Option<MapCoordinate> targetCoordinate, Action sfxCallback)
        {
            _walkPath.Clear();

            targetCoordinate.Match(
                some: tc =>
                {
                    _targetCoordinate = targetCoordinate;

                    var rp = _characterRepository.MainCharacter.RenderProperties;
                    var characterCoord = new MapCoordinate(rp.MapX, rp.MapY);

                    _walkPath = _pathFinder.FindPath(characterCoord, tc);

                    if (_walkPath.Any())
                    {
                        if (!_otherPlayerStartWalkingTimes.ContainsKey(_characterRepository.MainCharacter.ID))
                        {
                            rp = FaceTarget(characterCoord, _walkPath.Peek(), rp);
                            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(rp);
                        }

                        doTheWalk();
                    }
                },
                none: doTheWalk);

            void doTheWalk()
            {
                if (_otherPlayerStartWalkingTimes.ContainsKey(_characterRepository.MainCharacter.ID))
                {
                    _otherPlayerStartWalkingTimes[_characterRepository.MainCharacter.ID].Replay = true;
                    return;
                }

                var startWalkingTime = new RenderFrameActionTime(_characterRepository.MainCharacter.ID, sfxCallback);
                _otherPlayerStartWalkingTimes.Add(_characterRepository.MainCharacter.ID, startWalkingTime);

                _characterActions.Walk();
                startWalkingTime.SoundEffect();
            }
        }

        public void StartMainCharacterAttackAnimation()
        {
            if (_otherPlayerStartAttackingTimes.ContainsKey(_characterRepository.MainCharacter.ID))
            {
                _otherPlayerStartAttackingTimes[_characterRepository.MainCharacter.ID].Replay = true;
                return;
            }

            var startAttackingTime = new RenderFrameActionTime(_characterRepository.MainCharacter.ID);
            _otherPlayerStartAttackingTimes.Add(_characterRepository.MainCharacter.ID, startAttackingTime);
        }

        public bool MainCharacterShoutSpellPrep(ESFRecord spellData, ISpellTargetable target)
        {
            if (_mainPlayerStartShoutTime.HasValue)
                return false;

            _mainPlayerStartShoutTime = Option.Some(Stopwatch.StartNew());
            _shoutSpellData = spellData;
            _spellTarget = target;
            return true;
        }

        public void MainCharacterCancelSpellPrep()
        {
            _mainPlayerStartShoutTime = Option.None<Stopwatch>();
            _shoutSpellData = null;
            _spellTarget = null;

            _spellSlotDataRepository.SelectedSpellSlot = Option.None<int>();
            _spellSlotDataRepository.SpellIsPrepared = false;
        }

        public void StartOtherCharacterWalkAnimation(int characterID, byte destinationX, byte destinationY, EODirection direction)
        {
            if (_otherPlayerStartWalkingTimes.ContainsKey(characterID))
            {
                _otherPlayerStartWalkingTimes[characterID].Replay = true;
                _queuedDirections[characterID] = direction;
                _queuedPositions[characterID] = new MapCoordinate(destinationX, destinationY);
                return;
            }

            var startWalkingTimeAndID = new RenderFrameActionTime(characterID);
            _otherPlayerStartWalkingTimes.Add(characterID, startWalkingTimeAndID);
        }

        public void StartOtherCharacterAttackAnimation(int characterID)
        {
            if (_otherPlayerStartAttackingTimes.ContainsKey(characterID))
            {
                _otherPlayerStartAttackingTimes[characterID].Replay = true;
                return;
            }

            var startAttackingTime = new RenderFrameActionTime(characterID);
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

            var startAttackingTimeAndID = new RenderFrameActionTime(characterID);
            _otherPlayerStartSpellCastTimes.Add(characterID, startAttackingTimeAndID);
        }

        public bool Emote(int characterID, Emote whichEmote)
        {
            if (((_otherPlayerStartWalkingTimes.ContainsKey(characterID) ||
                _otherPlayerStartAttackingTimes.ContainsKey(characterID) ||
                _otherPlayerStartSpellCastTimes.ContainsKey(characterID)) && whichEmote != EOLib.Domain.Character.Emote.LevelUp) ||
                _startEmoteTimes.ContainsKey(characterID))
                return false;

            var startEmoteTime = new RenderFrameActionTime(characterID);
            if (characterID == _characterRepository.MainCharacter.ID)
            {
                var rp = _characterRepository.MainCharacter.RenderProperties.WithEmote(whichEmote);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(rp);
            }
            else if (_currentMapStateRepository.Characters.TryGetValue(characterID, out var otherCharacter))
            {
                var rp = otherCharacter.RenderProperties.WithEmote(whichEmote);
                _currentMapStateRepository.Characters[characterID] = otherCharacter.WithRenderProperties(rp);
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add((short)characterID);
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

            _currentMapStateRepository.Characters = _currentMapStateRepository.Characters.Values
                .Select(c => c.WithRenderProperties(c.RenderProperties.ResetAnimationFrames()))
                .ToDictionary(k => k.ID, v => v);
        }

        #region Walk Animation

        private void AnimateCharacterWalking()
        {
            var playersDoneWalking = new List<int>();
            foreach (var pair in _otherPlayerStartWalkingTimes.Values)
            {
                var sendWalk = false;

                if (pair.ActionTimer.ElapsedMilliseconds >= WALK_FRAME_TIME_MS)
                {
                    GetCurrentCharacterFromRepository(pair).Match(
                        none: () => playersDoneWalking.Add(pair.UniqueID),
                        some: currentCharacter =>
                        {
                            var renderProperties = currentCharacter.RenderProperties;
                            var nextFrameRenderProperties = AnimateOneWalkFrame(renderProperties);

                            pair.UpdateActionStartTime();
                            if (nextFrameRenderProperties.IsActing(CharacterActionState.Standing))
                            {
                                var isMainCharacter = currentCharacter == _characterRepository.MainCharacter;
                                var canMoveToDestinationCoordinates = _walkValidationActions.CanMoveToCoordinates(nextFrameRenderProperties.GetDestinationX(), nextFrameRenderProperties.GetDestinationY());

                                if (pair.Replay)
                                {
                                    if (!isMainCharacter || (isMainCharacter && canMoveToDestinationCoordinates))
                                    {
                                        // send the walk packet after the game state has been updated so the correct coordinates are sent
                                        sendWalk = isMainCharacter;
                                        var extraFrameProps = AnimateOneWalkFrame(nextFrameRenderProperties.ResetAnimationFrames());
                                        pair.Replay = false;

                                        if (_queuedDirections.ContainsKey(pair.UniqueID))
                                        {
                                            extraFrameProps = extraFrameProps.WithDirection(_queuedDirections[pair.UniqueID]);
                                            _queuedDirections.Remove(pair.UniqueID);

                                            canMoveToDestinationCoordinates = _walkValidationActions.CanMoveToCoordinates(extraFrameProps.GetDestinationX(), extraFrameProps.GetDestinationY());
                                            if (!canMoveToDestinationCoordinates)
                                            {
                                                playersDoneWalking.Add(pair.UniqueID);
                                                sendWalk = false;
                                            }
                                            else
                                            {
                                                nextFrameRenderProperties = extraFrameProps;
                                            }
                                        }
                                        else
                                        {
                                            nextFrameRenderProperties = extraFrameProps;
                                        }

                                        if (sendWalk)
                                        {
                                            pair.SoundEffect();
                                        }
                                    }
                                    else
                                    {
                                        // tried to replay but the new destination position is not walkable
                                        playersDoneWalking.Add(pair.UniqueID);
                                    }
                                }
                                else if (isMainCharacter && _walkPath.Any() && !_clickHandled)
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

                                _clickHandled = false;
                            }

                            var nextFrameCharacter = currentCharacter.WithRenderProperties(nextFrameRenderProperties);
                            UpdateCharacterInRepository(currentCharacter, nextFrameCharacter);

                            if (sendWalk)
                            {
                                _characterActions.Walk();
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
                if (pair.ActionTimer.ElapsedMilliseconds >= ATTACK_FRAME_TIME_MS)
                {
                    GetCurrentCharacterFromRepository(pair).Match(
                        none: () => playersDoneAttacking.Add(pair.UniqueID),
                        some: currentCharacter =>
                        {
                            var renderProperties = currentCharacter.RenderProperties;
                            var nextFrameRenderProperties = renderProperties.WithNextAttackFrame();

                            pair.UpdateActionStartTime();
                            if (nextFrameRenderProperties.IsActing(CharacterActionState.Standing))
                            {
                                if (pair.Replay)
                                {
                                    nextFrameRenderProperties = renderProperties.ResetAnimationFrames()
                                        .WithNextAttackFrame();
                                    pair.Replay = false;
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
            _mainPlayerStartShoutTime.MatchSome(t =>
            {
                // todo: math ???
                // grabbed this formula from OldCharacterRenderer but it isn't certain that this is correct
                if (t.ElapsedMilliseconds > (int)Math.Round(_shoutSpellData.CastTime / 2.0 * 950))
                {
                    _otherPlayerStartSpellCastTimes.Add(_characterRepository.MainCharacter.ID, new RenderFrameActionTime(_characterRepository.MainCharacter.ID));
                    _characterActions.CastSpell(_shoutSpellData.ID, _spellTarget);
                    MainCharacterCancelSpellPrep();
                }
            });

            var playersDoneCasting = new HashSet<int>();
            foreach (var pair in _otherPlayerStartSpellCastTimes.Values)
            {
                if (pair.ActionTimer.ElapsedMilliseconds >= ATTACK_FRAME_TIME_MS)
                {
                    GetCurrentCharacterFromRepository(pair).Match(
                        none: () => playersDoneCasting.Add(pair.UniqueID),
                        some: currentCharacter =>
                        {
                            var renderProperties = currentCharacter.RenderProperties;
                            var nextFrameRenderProperties = renderProperties.WithNextSpellCastFrame();

                            pair.UpdateActionStartTime();
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
                if (pair.ActionTimer.ElapsedMilliseconds >= EMOTE_FRAME_TIME_MS)
                {
                    GetCurrentCharacterFromRepository(pair).Match(
                        none: () => playersDoneEmoting.Add(pair.UniqueID),
                        some: currentCharacter =>
                        {
                            var renderProperties = currentCharacter.RenderProperties;
                            var nextFrameRenderProperties = renderProperties.WithNextEmoteFrame();

                            pair.UpdateActionStartTime();
                            if (nextFrameRenderProperties.IsActing(CharacterActionState.Standing))
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
                : _currentMapStateRepository.Characters.ContainsKey(pair.UniqueID)
                    ? Option.Some(_currentMapStateRepository.Characters[pair.UniqueID])
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
                _currentMapStateRepository.Characters[nextFrameCharacter.ID] = nextFrameCharacter;
            }
        }

        private void ResetCharacterAnimationFrames(int characterID)
        {
            var character = _currentMapStateRepository.Characters[characterID];
            var renderProps = character.RenderProperties.ResetAnimationFrames();
            var newCharacter = character.WithRenderProperties(renderProps);
            _currentMapStateRepository.Characters[characterID] = newCharacter;
        }
    }

    public interface ICharacterAnimator : IGameComponent
    {
        void MainCharacterFace(EODirection direction);

        void StartMainCharacterWalkAnimation(Option<MapCoordinate> targetCoordinate, Action sfxCallback);

        void StartMainCharacterAttackAnimation();

        bool MainCharacterShoutSpellPrep(ESFRecord spellData, ISpellTargetable spellTarget);

        void MainCharacterCancelSpellPrep();

        void StartOtherCharacterWalkAnimation(int characterID, byte targetX, byte targetY, EODirection direction);

        void StartOtherCharacterAttackAnimation(int characterID);

        void StartOtherCharacterSpellCast(int characterID);

        bool Emote(int characterID, Emote whichEmote);

        void StopAllCharacterAnimations();
    }
}
