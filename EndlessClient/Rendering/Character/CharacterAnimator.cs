using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Character
{
    public class CharacterAnimator : GameComponent, ICharacterAnimator
    {
        public const int WALK_FRAME_TIME_MS = 100;
        public const int ATTACK_FRAME_TIME_MS = 285;

        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICharacterActions _characterActions;

        private readonly Dictionary<int, EODirection> _queuedDirections;
        private readonly Dictionary<int, MapCoordinate> _queuedPositions;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartWalkingTimes;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartAttackingTimes;
        private readonly Dictionary<int, RenderFrameActionTime> _otherPlayerStartSpellCastTimes;

        public CharacterAnimator(IEndlessGameProvider gameProvider,
                                 ICharacterRepository characterRepository,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 ICurrentMapProvider currentMapProvider,
                                 ICharacterActions characterActions)
            : base((Game) gameProvider.Game)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _currentMapProvider = currentMapProvider;
            _characterActions = characterActions;

            _queuedDirections = new Dictionary<int, EODirection>();
            _queuedPositions = new Dictionary<int, MapCoordinate>();
            _otherPlayerStartWalkingTimes = new Dictionary<int, RenderFrameActionTime>();
            _otherPlayerStartAttackingTimes = new Dictionary<int, RenderFrameActionTime>();
            _otherPlayerStartSpellCastTimes = new Dictionary<int, RenderFrameActionTime>();
        }

        public override void Update(GameTime gameTime)
        {
            AnimateCharacterWalking();
            AnimateCharacterAttacking();
            AnimateCharacterSpells();

            base.Update(gameTime);
        }

        public bool IsAttacking(int characterId)
        {
            return _otherPlayerStartAttackingTimes.ContainsKey(characterId);
        }

        public void MainCharacterFace(EODirection direction)
        {
            if (_otherPlayerStartWalkingTimes.ContainsKey(_characterRepository.MainCharacter.ID))
            {
                _queuedDirections[_characterRepository.MainCharacter.ID] = direction;
                return;
            }

            var renderProperties = _characterRepository.MainCharacter.RenderProperties;
            renderProperties = renderProperties.WithDirection(direction);

            var newMainCharacter = _characterRepository.MainCharacter.WithRenderProperties(renderProperties);
            _characterRepository.MainCharacter = newMainCharacter;
        }

        public void StartMainCharacterWalkAnimation()
        {
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
            if (_otherPlayerStartWalkingTimes.ContainsKey(characterID) ||
                _otherPlayerStartSpellCastTimes.ContainsKey(characterID))
                return;

            if (_otherPlayerStartAttackingTimes.TryGetValue(characterID, out var _))
            {
                ResetCharacterAnimationFrames(characterID);
                _otherPlayerStartAttackingTimes.Remove(characterID);
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

            _characterRepository.MainCharacter =
                _characterRepository.MainCharacter.WithRenderProperties(
                    _characterRepository.MainCharacter.RenderProperties.ResetAnimationFrames());

            _currentMapStateRepository.Characters =
                new HashSet<ICharacter>(
                    _currentMapStateRepository.Characters.Select(x => x.WithRenderProperties(x.RenderProperties.ResetAnimationFrames())));
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
                    var currentCharacter = GetCurrentCharacterFromRepository(pair);
                    if (currentCharacter == null)
                    {
                        playersDoneWalking.Add(pair.UniqueID);
                        continue;
                    }

                    var renderProperties = currentCharacter.RenderProperties;
                    var nextFrameRenderProperties = AnimateOneWalkFrame(renderProperties);

                    pair.UpdateActionStartTime();
                    if (nextFrameRenderProperties.IsActing(CharacterActionState.Standing))
                    {
                        if (pair.Replay)
                        {
                            // send the walk packet after the game state has been updated so the correct coordinates are sent
                            sendWalk = currentCharacter == _characterRepository.MainCharacter;
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
                }
            }

            foreach (var key in playersDoneWalking)
                _otherPlayerStartWalkingTimes.Remove(key);
        }

        private ICharacterRenderProperties AnimateOneWalkFrame(ICharacterRenderProperties renderProperties)
        {
            var isSteppingStone = (IsInBounds(renderProperties, false) && _currentMapProvider.CurrentMap.Tiles[renderProperties.MapY, renderProperties.MapX] == TileSpec.Jump)
                || (IsInBounds(renderProperties, true) && _currentMapProvider.CurrentMap.Tiles[renderProperties.GetDestinationY(), renderProperties.GetDestinationX()] == TileSpec.Jump);

            var nextFrameRenderProperties = renderProperties.WithNextWalkFrame(isSteppingStone);
            if (nextFrameRenderProperties.CurrentAction != CharacterActionState.Walking)
            {
                nextFrameRenderProperties = nextFrameRenderProperties
                    .WithMapX(nextFrameRenderProperties.GetDestinationX())
                    .WithMapY(nextFrameRenderProperties.GetDestinationY());
            }

            return nextFrameRenderProperties;
        }

        private bool IsInBounds(ICharacterRenderProperties renderProperties, bool dest)
        {
            var mapProps = _currentMapProvider.CurrentMap.Properties;
            var x = dest ? renderProperties.GetDestinationX() : renderProperties.MapX;
            var y = dest ? renderProperties.GetDestinationY() : renderProperties.MapY;
            return x < mapProps.Width && x >= 0 && y < mapProps.Height && y >= 0;
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
                    var currentCharacter = GetCurrentCharacterFromRepository(pair);
                    if (currentCharacter == null)
                    {
                        playersDoneAttacking.Add(pair.UniqueID);
                        continue;
                    }

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
                    var currentCharacter = GetCurrentCharacterFromRepository(pair);
                    if (currentCharacter == null)
                    {
                        playersDoneCasting.Add(pair.UniqueID);
                        continue;
                    }

                    var renderProperties = currentCharacter.RenderProperties;
                    var nextFrameRenderProperties = renderProperties.WithNextSpellCastFrame();

                    pair.UpdateActionStartTime();
                    if (nextFrameRenderProperties.IsActing(CharacterActionState.Standing))
                        playersDoneCasting.Add(pair.UniqueID);

                    var nextFrameCharacter = currentCharacter.WithRenderProperties(nextFrameRenderProperties);
                    UpdateCharacterInRepository(currentCharacter, nextFrameCharacter);
                }
            }

            foreach (var key in playersDoneCasting)
                _otherPlayerStartSpellCastTimes.Remove(key);
        }

        #endregion

        private ICharacter GetCurrentCharacterFromRepository(RenderFrameActionTime pair)
        {
            return pair.UniqueID == _characterRepository.MainCharacter.ID
                ? _characterRepository.MainCharacter
                : _currentMapStateRepository.Characters.SingleOrDefault(x => x.ID == pair.UniqueID);
        }

        private void UpdateCharacterInRepository(ICharacter currentCharacter, ICharacter nextFrameCharacter)
        {
            if (currentCharacter == _characterRepository.MainCharacter)
            {
                _characterRepository.MainCharacter = nextFrameCharacter;
            }
            else
            {
                _currentMapStateRepository.Characters.Remove(currentCharacter);
                _currentMapStateRepository.Characters.Add(nextFrameCharacter);
            }
        }

        private void ResetCharacterAnimationFrames(int characterID)
        {
            var character = _currentMapStateRepository.Characters.Single(x => x.ID == characterID);
            var renderProps = character.RenderProperties.ResetAnimationFrames();
            var newCharacter = character.WithRenderProperties(renderProps);
            _currentMapStateRepository.Characters.Remove(character);
            _currentMapStateRepository.Characters.Add(newCharacter);
        }
    }

    public interface ICharacterAnimator : IGameComponent
    {
        bool IsAttacking(int characterId);

        void MainCharacterFace(EODirection direction);

        void StartMainCharacterWalkAnimation();

        void StartMainCharacterAttackAnimation();

        void StartOtherCharacterWalkAnimation(int characterID, byte targetX, byte targetY, EODirection direction);

        void StartOtherCharacterAttackAnimation(int characterID);

        void StartOtherCharacterSpellCast(int characterID);

        void StopAllCharacterAnimations();
    }
}
