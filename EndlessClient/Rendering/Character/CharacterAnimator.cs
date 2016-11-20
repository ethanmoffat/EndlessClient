// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Character
{
    public class CharacterAnimator : GameComponent, ICharacterAnimator
    {
        public const int WALK_FRAME_TIME_MS = 100;
        public const int ATTACK_FRAME_TIME_MS = 285;

        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        private Optional<DateTime> _startWalkingTime;
        private readonly List<RenderFrameActionTime> _otherPlayerStartWalkingTimes;

        private Optional<DateTime> _startAttackingTime;
        private readonly List<RenderFrameActionTime> _otherPlayerStartAttackingTimes;

        public bool MainCharacterIsAttacking { get { return _startAttackingTime.HasValue; } }

        public CharacterAnimator(IEndlessGameProvider gameProvider,
                                 ICharacterRepository characterRepository,
                                 ICurrentMapStateRepository currentMapStateRepository)
            : base((Game) gameProvider.Game)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;

            _startWalkingTime = Optional<DateTime>.Empty;
            _otherPlayerStartWalkingTimes = new List<RenderFrameActionTime>();

            _startAttackingTime = Optional<DateTime>.Empty;
            _otherPlayerStartAttackingTimes = new List<RenderFrameActionTime>();
        }

        public override void Update(GameTime gameTime)
        {
            var now = DateTime.Now;

            AnimateCharacterWalking(now);
            AnimateCharacterAttacking(now);

            base.Update(gameTime);
        }

        public void StartMainCharacterWalkAnimation()
        {
            if (_startWalkingTime.HasValue) return;
            _startWalkingTime = DateTime.Now;
        }

        public void StartMainCharacterAttackAnimation()
        {
            //todo: animation is currently really choppy, make it smoother
            var renderProperties = _characterRepository.MainCharacter.RenderProperties;
            if (_startAttackingTime.HasValue &&
                renderProperties.AttackFrame != CharacterRenderProperties.MAX_NUMBER_OF_ATTACK_FRAMES)
                return;

            _startAttackingTime = DateTime.Now;
        }

        public void StartOtherCharacterWalkAnimation(int characterID)
        {
            if (_otherPlayerStartWalkingTimes.Any(x => x.UniqueID == characterID) ||
                _otherPlayerStartAttackingTimes.Any(x => x.UniqueID == characterID))
                return;

            var startWalkingTimeAndID = new RenderFrameActionTime(characterID, DateTime.Now);

            _otherPlayerStartWalkingTimes.Add(startWalkingTimeAndID);
        }

        public void StartOtherCharacterAttackAnimation(int characterID)
        {
            if (_otherPlayerStartWalkingTimes.Any(x => x.UniqueID == characterID) ||
                _otherPlayerStartAttackingTimes.Any(x => x.UniqueID == characterID))
                return;

            var startAttackingTimeAndID = new RenderFrameActionTime(characterID, DateTime.Now);

            _otherPlayerStartAttackingTimes.Add(startAttackingTimeAndID);
        }

        public void StopAllOtherCharacterAnimations()
        {
            _otherPlayerStartWalkingTimes.Clear();
            _otherPlayerStartAttackingTimes.Clear();
        }

        #region Walk Animation

        private void AnimateCharacterWalking(DateTime now)
        {
            if (_startWalkingTime.HasValue &&
                (now - _startWalkingTime).TotalMilliseconds > WALK_FRAME_TIME_MS)
            {
                var renderProperties = _characterRepository.MainCharacter.RenderProperties;
                var nextFrameRenderProperties = AnimateOneWalkFrame(renderProperties);

                _startWalkingTime = GetUpdatedActionTime(now, nextFrameRenderProperties);

                var nextFrameCharacter = _characterRepository.MainCharacter.WithRenderProperties(nextFrameRenderProperties);
                _characterRepository.MainCharacter = nextFrameCharacter;
            }

            var playersDoneWalking = new List<RenderFrameActionTime>();
            foreach (var pair in _otherPlayerStartWalkingTimes)
            {
                if (pair.ActionStartTime.HasValue &&
                    (now - pair.ActionStartTime).TotalMilliseconds > WALK_FRAME_TIME_MS)
                {
                    var currentCharacter = _currentMapStateRepository.Characters.Single(x => x.ID == pair.UniqueID);

                    var renderProperties = currentCharacter.RenderProperties;
                    var nextFrameRenderProperties = AnimateOneWalkFrame(renderProperties);

                    pair.UpdateActionStartTime(GetUpdatedActionTime(now, nextFrameRenderProperties));
                    if (!pair.ActionStartTime.HasValue)
                        playersDoneWalking.Add(pair);

                    var nextFrameCharacter = currentCharacter.WithRenderProperties(nextFrameRenderProperties);
                    _currentMapStateRepository.Characters.Remove(currentCharacter);
                    _currentMapStateRepository.Characters.Add(nextFrameCharacter);
                }
            }
            _otherPlayerStartWalkingTimes.RemoveAll(playersDoneWalking.Contains);
        }

        private static ICharacterRenderProperties AnimateOneWalkFrame(ICharacterRenderProperties renderProperties)
        {
            var nextFrameRenderProperties = renderProperties.WithNextWalkFrame();

            if (nextFrameRenderProperties.CurrentAction != CharacterActionState.Walking)
            {
                nextFrameRenderProperties = nextFrameRenderProperties
                    .WithMapX(nextFrameRenderProperties.GetDestinationX())
                    .WithMapY(nextFrameRenderProperties.GetDestinationY());
            }

            return nextFrameRenderProperties;
        }

        #endregion

        #region Attack Animation

        private void AnimateCharacterAttacking(DateTime now)
        {
            if (_startAttackingTime.HasValue &&
                (now - _startAttackingTime).TotalMilliseconds > ATTACK_FRAME_TIME_MS)
            {
                var renderProperties = _characterRepository.MainCharacter.RenderProperties;
                var nextFrameRenderProperties = renderProperties.WithNextAttackFrame();

                _startAttackingTime = GetUpdatedActionTime(now, nextFrameRenderProperties);

                var nextFrameCharacter = _characterRepository.MainCharacter.WithRenderProperties(nextFrameRenderProperties);
                _characterRepository.MainCharacter = nextFrameCharacter;
            }

            var playersDoneAttacking = new List<RenderFrameActionTime>();
            foreach (var pair in _otherPlayerStartAttackingTimes)
            {
                if (pair.ActionStartTime.HasValue &&
                    (now - pair.ActionStartTime).TotalMilliseconds > ATTACK_FRAME_TIME_MS)
                {
                    var currentCharacter = _currentMapStateRepository.Characters.Single(x => x.ID == pair.UniqueID);

                    var renderProperties = currentCharacter.RenderProperties;
                    var nextFrameRenderProperties = renderProperties.WithNextAttackFrame();

                    pair.UpdateActionStartTime(GetUpdatedActionTime(now, nextFrameRenderProperties));
                    if (!pair.ActionStartTime.HasValue)
                        playersDoneAttacking.Add(pair);

                    var nextFrameCharacter = currentCharacter.WithRenderProperties(nextFrameRenderProperties);
                    _currentMapStateRepository.Characters.Remove(currentCharacter);
                    _currentMapStateRepository.Characters.Add(nextFrameCharacter);
                }
            }
            _otherPlayerStartAttackingTimes.RemoveAll(playersDoneAttacking.Contains);
        }

        #endregion

        private static Optional<DateTime> GetUpdatedActionTime(DateTime now, ICharacterRenderProperties nextFrameRenderProperties)
        {
            return nextFrameRenderProperties.IsActing(CharacterActionState.Standing)
                ? Optional<DateTime>.Empty
                : new Optional<DateTime>(now);
        }
    }

    public interface ICharacterAnimator : IGameComponent
    {
        bool MainCharacterIsAttacking { get; }

        void StartMainCharacterWalkAnimation();

        void StartMainCharacterAttackAnimation();

        void StartOtherCharacterWalkAnimation(int characterID);

        void StartOtherCharacterAttackAnimation(int characterID);

        void StopAllOtherCharacterAnimations();
    }
}
