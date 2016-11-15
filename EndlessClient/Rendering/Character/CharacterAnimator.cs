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

        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        private Optional<DateTime> _startWalkingTime;
        private readonly List<RenderFrameActionTime> _otherPlayerStartWalkingTimes;

        public CharacterAnimator(IEndlessGameProvider gameProvider,
                                 ICharacterRepository characterRepository,
                                 ICurrentMapStateRepository currentMapStateRepository)
            : base((Game) gameProvider.Game)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _startWalkingTime = Optional<DateTime>.Empty;
            _otherPlayerStartWalkingTimes = new List<RenderFrameActionTime>();
        }

        public override void Update(GameTime gameTime)
        {
            var now = DateTime.Now;

            AnimateCharacterWalking(now);

            base.Update(gameTime);
        }

        public void StartMainCharacterWalkAnimation()
        {
            _startWalkingTime = DateTime.Now;
        }

        public void StartOtherCharacterWalkAnimation(int characterID)
        {
            var startWalkingTimeAndID = new RenderFrameActionTime(characterID, DateTime.Now);

            _otherPlayerStartWalkingTimes.Add(startWalkingTimeAndID);
            if (_otherPlayerStartWalkingTimes.Select(x => x.UniqueID).Distinct().Count() != _otherPlayerStartWalkingTimes.Count)
                throw new InvalidOperationException("NPC is trying to walk twice! figure something out for this case");
        }

        private void AnimateCharacterWalking(DateTime now)
        {
            if (_startWalkingTime.HasValue &&
                (now - _startWalkingTime).TotalMilliseconds > WALK_FRAME_TIME_MS)
            {
                var renderProperties = _characterRepository.MainCharacter.RenderProperties;
                var nextFrameRenderProperties = AnimateOneWalkFrame(renderProperties);

                _startWalkingTime = GetUpdatedStartWalkingTime(now, nextFrameRenderProperties);

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

                    pair.UpdateActionStartTime(GetUpdatedStartWalkingTime(now, nextFrameRenderProperties));
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

        private static Optional<DateTime> GetUpdatedStartWalkingTime(DateTime now, ICharacterRenderProperties nextFrameRenderProperties)
        {
            return nextFrameRenderProperties.IsActing(CharacterActionState.Standing)
                ? Optional<DateTime>.Empty
                : new Optional<DateTime>(now);
        }
    }

    public interface ICharacterAnimator : IGameComponent
    {
        void StartMainCharacterWalkAnimation();

        void StartOtherCharacterWalkAnimation(int characterID);
    }
}
