// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Character
{
    public class CharacterStateUpdater : GameComponent
    {
        public const int WALK_FRAME_TIME_MS = 100;

        private readonly ICharacterRepository _characterRepository;

        private Optional<DateTime> _startWalkingTime;

        public CharacterStateUpdater(IEndlessGameProvider gameProvider,
                                     ICharacterRepository characterRepository)
            : base((Game) gameProvider.Game)
        {
            _characterRepository = characterRepository;
            _startWalkingTime = Optional<DateTime>.Empty;
        }

        public override void Update(GameTime gameTime)
        {
            var now = DateTime.Now;

            if (_startWalkingTime != null && _startWalkingTime.HasValue &&
                (now - _startWalkingTime).TotalMilliseconds > WALK_FRAME_TIME_MS)
            {
                var nextFrameRenderProperties = RenderProperties.WithNextWalkFrame();
                var nextFrameCharacter = _characterRepository.MainCharacter.WithRenderProperties(nextFrameRenderProperties);
                _characterRepository.MainCharacter = nextFrameCharacter;

                if (nextFrameCharacter.RenderProperties.CurrentAction != CharacterActionState.Walking)
                    _startWalkingTime = Optional<DateTime>.Empty;
            }

            base.Update(gameTime);
        }

        public void StartWalking()
        {
            _startWalkingTime = DateTime.Now;
        }

        private ICharacterRenderProperties RenderProperties
        {
            get { return _characterRepository.MainCharacter.RenderProperties; }
        }
    }
}
