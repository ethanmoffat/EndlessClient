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

        private readonly List<RenderFrameActionTime> _otherPlayerStartWalkingTimes;
        private readonly List<RenderFrameActionTime> _otherPlayerStartAttackingTimes;

        public CharacterAnimator(IEndlessGameProvider gameProvider,
                                 ICharacterRepository characterRepository,
                                 ICurrentMapStateRepository currentMapStateRepository)
            : base((Game) gameProvider.Game)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;

            _otherPlayerStartWalkingTimes = new List<RenderFrameActionTime>();

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
            if (_otherPlayerStartWalkingTimes.Any(HasActionForMainCharacter))
                return;

            var startWalkingTime = new RenderFrameActionTime(_characterRepository.MainCharacter.ID, GetStartingAnimationTime(WALK_FRAME_TIME_MS));
            _otherPlayerStartWalkingTimes.Add(startWalkingTime);
        }

        public void StartMainCharacterAttackAnimation()
        {
            if (_otherPlayerStartAttackingTimes.Any(HasActionForMainCharacter))
                return;

            var startAttackingTime = new RenderFrameActionTime(_characterRepository.MainCharacter.ID, GetStartingAnimationTime(ATTACK_FRAME_TIME_MS));
            _otherPlayerStartAttackingTimes.Add(startAttackingTime);
        }

        public void StartOtherCharacterWalkAnimation(int characterID)
        {
            if (_otherPlayerStartAttackingTimes.Any(x => x.UniqueID == characterID))
                return;

            var existingStartTime = _otherPlayerStartWalkingTimes.SingleOrDefault(x => x.UniqueID == characterID);
            if (existingStartTime != null)
            {
                ResetCharacterAnimationFrames(characterID);
                _otherPlayerStartWalkingTimes.Remove(existingStartTime);
            }

            var startWalkingTimeAndID = new RenderFrameActionTime(characterID, GetStartingAnimationTime(WALK_FRAME_TIME_MS));
            _otherPlayerStartWalkingTimes.Add(startWalkingTimeAndID);
        }

        public void StartOtherCharacterAttackAnimation(int characterID)
        {
            if (_otherPlayerStartWalkingTimes.Any(x => x.UniqueID == characterID))
                return;

            var existingStartTime = _otherPlayerStartAttackingTimes.SingleOrDefault(x => x.UniqueID == characterID);
            if (existingStartTime != null)
            {
                ResetCharacterAnimationFrames(characterID);
                _otherPlayerStartAttackingTimes.Remove(existingStartTime);
            }

            var startAttackingTimeAndID = new RenderFrameActionTime(characterID, GetStartingAnimationTime(ATTACK_FRAME_TIME_MS));
            _otherPlayerStartAttackingTimes.Add(startAttackingTimeAndID);
        }

        public void StopAllCharacterAnimations()
        {
            _otherPlayerStartWalkingTimes.Clear();
            _otherPlayerStartAttackingTimes.Clear();

            _characterRepository.MainCharacter =
                _characterRepository.MainCharacter.WithRenderProperties(
                    _characterRepository.MainCharacter.RenderProperties.ResetAnimationFrames());

            _currentMapStateRepository.Characters =
                _currentMapStateRepository.Characters.Select(x => x.WithRenderProperties(x.RenderProperties.ResetAnimationFrames()))
                    .ToList();
        }

        #region Walk Animation

        private void AnimateCharacterWalking(DateTime now)
        {
            var playersDoneWalking = new List<RenderFrameActionTime>();
            foreach (var pair in _otherPlayerStartWalkingTimes)
            {
                if (pair.ActionStartTime.HasValue &&
                    (now - pair.ActionStartTime).TotalMilliseconds > WALK_FRAME_TIME_MS)
                {
                    var currentCharacter = GetCurrentCharacterFromRepository(pair);
                    if (currentCharacter == null)
                    {
                        playersDoneWalking.Add(pair);
                        continue;
                    }

                    var renderProperties = currentCharacter.RenderProperties;
                    var nextFrameRenderProperties = AnimateOneWalkFrame(renderProperties);

                    pair.UpdateActionStartTime(GetUpdatedActionTime(now, nextFrameRenderProperties));
                    if (!pair.ActionStartTime.HasValue)
                        playersDoneWalking.Add(pair);

                    var nextFrameCharacter = currentCharacter.WithRenderProperties(nextFrameRenderProperties);
                    UpdateCharacterInRepository(currentCharacter, nextFrameCharacter);
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
            var playersDoneAttacking = new List<RenderFrameActionTime>();
            foreach (var pair in _otherPlayerStartAttackingTimes)
            {
                if (pair.ActionStartTime.HasValue &&
                    (now - pair.ActionStartTime).TotalMilliseconds > ATTACK_FRAME_TIME_MS)
                {
                    var currentCharacter = GetCurrentCharacterFromRepository(pair);
                    if (currentCharacter == null)
                    {
                        playersDoneAttacking.Add(pair);
                        continue;
                    }

                    var renderProperties = currentCharacter.RenderProperties;
                    var nextFrameRenderProperties = renderProperties.WithNextAttackFrame();

                    pair.UpdateActionStartTime(GetUpdatedActionTime(now, nextFrameRenderProperties));
                    if (!pair.ActionStartTime.HasValue)
                        playersDoneAttacking.Add(pair);

                    var nextFrameCharacter = currentCharacter.WithRenderProperties(nextFrameRenderProperties);
                    UpdateCharacterInRepository(currentCharacter, nextFrameCharacter);
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

        private bool HasActionForMainCharacter(RenderFrameActionTime x)
        {
            return x.UniqueID == _characterRepository.MainCharacter.ID && x.ActionStartTime.HasValue;
        }

        private void ResetCharacterAnimationFrames(int characterID)
        {
            var character = _currentMapStateRepository.Characters.Single(x => x.ID == characterID);
            var renderProps = character.RenderProperties.ResetAnimationFrames();
            var newCharacter = character.WithRenderProperties(renderProps);
            _currentMapStateRepository.Characters.Remove(character);
            _currentMapStateRepository.Characters.Add(newCharacter);
        }

        private static DateTime GetStartingAnimationTime(int frameTimer)
        {
            // make the first frame very short for animation
            // this works around a bug where the first frame is delayed for (seemingly) no good reason
            // maybe I will eventually figure out why that was happening but this seems to work just fine for now
            return DateTime.Now.AddMilliseconds(-frameTimer);
        }
    }

    public interface ICharacterAnimator : IGameComponent
    {
        void StartMainCharacterWalkAnimation();

        void StartMainCharacterAttackAnimation();

        void StartOtherCharacterWalkAnimation(int characterID);

        void StartOtherCharacterAttackAnimation(int characterID);

        void StopAllCharacterAnimations();
    }
}
