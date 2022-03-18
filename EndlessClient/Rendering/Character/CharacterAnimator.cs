using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Rendering.Character
{
    public class CharacterAnimator : GameComponent, ICharacterAnimator
    {
        public const int WALK_FRAME_TIME_MS = 120;
        public const int ATTACK_FRAME_TIME_MS = 100;

        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICharacterActions _characterActions;
        private readonly IWalkValidationActions _walkValidationActions;
        private readonly IPathFinder _pathFinder;

        private readonly Dictionary<int, EODirection> _queuedDirections;
        private readonly Dictionary<int, MapCoordinate> _queuedPositions;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartWalkingTimes;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartAttackingTimes;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartSpellCastTimes;

        private Queue<MapCoordinate> _walkPath;
        private Option<MapCoordinate> _targetCoordinate;

        public CharacterAnimator(IEndlessGameProvider gameProvider,
                                 ICharacterRepository characterRepository,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 ICurrentMapProvider currentMapProvider,
                                 ICharacterActions characterActions,
                                 IWalkValidationActions walkValidationActions,
                                 IPathFinder pathFinder)
            : base((Game) gameProvider.Game)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _currentMapProvider = currentMapProvider;
            _characterActions = characterActions;
            _walkValidationActions = walkValidationActions;
            _pathFinder = pathFinder;
            _queuedDirections = new Dictionary<int, EODirection>();
            _queuedPositions = new Dictionary<int, MapCoordinate>();
            _otherPlayerStartWalkingTimes = new Dictionary<int, RenderFrameActionTime>();
            _otherPlayerStartAttackingTimes = new Dictionary<int, RenderFrameActionTime>();
            _otherPlayerStartSpellCastTimes = new Dictionary<int, RenderFrameActionTime>();
            _walkPath = new Queue<MapCoordinate>();
        }

        public override void Update(GameTime gameTime)
        {
            AnimateCharacterWalking();
            AnimateCharacterAttacking();
            AnimateCharacterSpells();

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

        public void StartMainCharacterWalkAnimation(Option<MapCoordinate> targetCoordinate)
        {
            _walkPath.Clear();
            targetCoordinate.MatchSome(tc =>
            {
                _targetCoordinate = targetCoordinate;

                var rp = _characterRepository.MainCharacter.RenderProperties;
                var characterCoord = new MapCoordinate(rp.MapX, rp.MapY);

                _walkPath = _pathFinder.FindPath(characterCoord, tc);

                if (!_otherPlayerStartWalkingTimes.ContainsKey(_characterRepository.MainCharacter.ID) && _walkPath.Any())
                {
                    rp = FaceTarget(characterCoord, _walkPath.Peek(), rp);
                    _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(rp);
                }
            });

            if (_otherPlayerStartWalkingTimes.ContainsKey(_characterRepository.MainCharacter.ID))
            {
                _otherPlayerStartWalkingTimes[_characterRepository.MainCharacter.ID].Replay = true;
                return;
            }

            var startWalkingTime = new RenderFrameActionTime(_characterRepository.MainCharacter.ID);
            _otherPlayerStartWalkingTimes.Add(_characterRepository.MainCharacter.ID, startWalkingTime);

            _characterActions.Walk();
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

        public void StartOtherCharacterWalkAnimation(int characterID, byte destinationX, byte destinationY, EODirection direction)
        {
            if (_otherPlayerStartWalkingTimes.TryGetValue(characterID, out var _))
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
            if (_otherPlayerStartAttackingTimes.TryGetValue(characterID, out var _))
            {
                _otherPlayerStartAttackingTimes[characterID].Replay = true;
                return;
            }

            var startAttackingTimeAndID = new RenderFrameActionTime(characterID);
            _otherPlayerStartAttackingTimes.Add(characterID, startAttackingTimeAndID);
        }

        public void StartOtherCharacterSpellCast(int characterID)
        {
            if (_otherPlayerStartWalkingTimes.ContainsKey(characterID) ||
                _otherPlayerStartAttackingTimes.ContainsKey(characterID))
                return;

            if (_otherPlayerStartSpellCastTimes.TryGetValue(characterID, out var _))
            {
                ResetCharacterAnimationFrames(characterID);
                _otherPlayerStartSpellCastTimes.Remove(characterID);
            }

            var startAttackingTimeAndID = new RenderFrameActionTime(characterID);
            _otherPlayerStartSpellCastTimes.Add(characterID, startAttackingTimeAndID);
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

                                if (pair.Replay)
                                {
                                    if (!isMainCharacter || (isMainCharacter && _walkValidationActions.CanMoveToCoordinates(nextFrameRenderProperties.GetDestinationX(), nextFrameRenderProperties.GetDestinationY())))
                                    {
                                        // send the walk packet after the game state has been updated so the correct coordinates are sent
                                        sendWalk = isMainCharacter;
                                        nextFrameRenderProperties = AnimateOneWalkFrame(nextFrameRenderProperties.ResetAnimationFrames());
                                        pair.Replay = false;

                                        if (_queuedDirections.ContainsKey(pair.UniqueID))
                                        {
                                            nextFrameRenderProperties = nextFrameRenderProperties.WithDirection(_queuedDirections[pair.UniqueID]);
                                            _queuedDirections.Remove(pair.UniqueID);
                                        }
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
                                        some: tc => _pathFinder.FindPath(characterCoord, tc),
                                        none: () => new Queue<MapCoordinate>());

                                    if (_walkPath.Any())
                                    {
                                        var next = _walkPath.Dequeue();
                                        nextFrameRenderProperties = FaceTarget(characterCoord, next, nextFrameRenderProperties);

                                        sendWalk = true;
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
                                _characterActions.Walk();
                            }
                        });
                }
            }

            foreach (var key in playersDoneWalking)
                _otherPlayerStartWalkingTimes.Remove(key);
        }

        private ICharacterRenderProperties AnimateOneWalkFrame(ICharacterRenderProperties renderProperties)
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

        private ICharacterRenderProperties FaceTarget(MapCoordinate characterCoord, MapCoordinate next, ICharacterRenderProperties rp)
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

        private Option<ICharacter> GetCurrentCharacterFromRepository(RenderFrameActionTime pair)
        {
            return pair.UniqueID == _characterRepository.MainCharacter.ID
                ? Option.Some(_characterRepository.MainCharacter)
                : _currentMapStateRepository.Characters.ContainsKey(pair.UniqueID)
                    ? Option.Some(_currentMapStateRepository.Characters[pair.UniqueID])
                    : Option.None<ICharacter>();
        }

        private void UpdateCharacterInRepository(ICharacter currentCharacter, ICharacter nextFrameCharacter)
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

        void StartMainCharacterWalkAnimation(Option<MapCoordinate> targetCoordinate);

        void StartMainCharacterAttackAnimation();

        void StartOtherCharacterWalkAnimation(int characterID, byte targetX, byte targetY, EODirection direction);

        void StartOtherCharacterAttackAnimation(int characterID);

        void StartOtherCharacterSpellCast(int characterID);

        void StopAllCharacterAnimations();
    }
}
