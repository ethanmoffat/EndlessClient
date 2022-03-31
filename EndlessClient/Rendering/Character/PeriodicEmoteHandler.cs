using EndlessClient.GameExecution;
using EndlessClient.Input;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Optional;
using System;
using System.Diagnostics;

namespace EndlessClient.Rendering.Character
{
    public class PeriodicEmoteHandler : GameComponent, IPeriodicEmoteHandler
    {
        private const int AFK_TIME_MINUTES = 5;
        private const int AFK_TIME_BETWEEN_EMOTES_MINUTES = 1;

        private readonly ICharacterActions _characterActions;
        private readonly IUserInputTimeProvider _userInputTimeProvider;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterAnimator _animator;

        private readonly Random _random;

        private Option<DateTime> _drunkStart;
        private Option<Stopwatch> _drunkTimeSinceLastEmote;
        private int _drunkIntervalSeconds;
        private double _drunkTimeoutSeconds;

        private Option<Stopwatch> _afkTimeSinceLastEmote;

        public PeriodicEmoteHandler(IEndlessGameProvider endlessGameProvider,
                                    ICharacterActions characterActions,
                                    IUserInputTimeProvider userInputTimeProvider,
                                    ICharacterRepository characterRepository,
                                    ICharacterAnimator animator)
            : base((Game)endlessGameProvider.Game)
        {
            _characterActions = characterActions;
            _userInputTimeProvider = userInputTimeProvider;
            _characterRepository = characterRepository;
            _animator = animator;

            _random = new Random();
        }

        public override void Update(GameTime gameTime)
        {
            _drunkStart.Match(
                some: ds =>
                {
                    if ((DateTime.Now - ds).TotalSeconds > _drunkTimeoutSeconds)
                    {
                        _drunkStart = Option.None<DateTime>();
                        _drunkTimeSinceLastEmote = Option.None<Stopwatch>();
                        _drunkIntervalSeconds = 0;

                        _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithRenderProperties(
                            _characterRepository.MainCharacter.RenderProperties.WithIsDrunk(false));
                    }
                    else
                    {
                        _drunkTimeSinceLastEmote.MatchSome(dt =>
                        {
                            if (dt.Elapsed.TotalSeconds > _drunkIntervalSeconds)
                            {
                                _drunkIntervalSeconds = _random.Next(4, 7);
                                _drunkTimeSinceLastEmote = Option.Some(Stopwatch.StartNew());

                                if (_animator.Emote(_characterRepository.MainCharacter.ID, Emote.Drunk))
                                    _characterActions.Emote(Emote.Drunk);
                            }
                        });
                    }
                },
                none: () =>
                {
                    if (_characterRepository.MainCharacter.RenderProperties.IsDrunk)
                    {
                        _drunkStart = Option.Some(DateTime.Now);
                        _drunkIntervalSeconds = _random.Next(4, 7);
                        _drunkTimeSinceLastEmote = Option.Some(Stopwatch.StartNew());

                        if (_animator.Emote(_characterRepository.MainCharacter.ID, Emote.Drunk))
                            _characterActions.Emote(Emote.Drunk);
                    }
                });

            if ((DateTime.Now - _userInputTimeProvider.LastInputTime).TotalMinutes >= AFK_TIME_MINUTES)
            {
                _afkTimeSinceLastEmote.Match(
                    some: at =>
                    {
                        if (at.Elapsed.TotalMinutes >= AFK_TIME_BETWEEN_EMOTES_MINUTES)
                        {
                            if (_animator.Emote(_characterRepository.MainCharacter.ID, Emote.Moon))
                                _characterActions.Emote(Emote.Moon);
                            _afkTimeSinceLastEmote = Option.Some(Stopwatch.StartNew());
                        }
                    },
                    none: () =>
                    {
                        if (_animator.Emote(_characterRepository.MainCharacter.ID, Emote.Moon))
                            _characterActions.Emote(Emote.Moon);
                        _afkTimeSinceLastEmote = Option.Some(Stopwatch.StartNew());
                    });
            }

            base.Update(gameTime);
        }

        public void SetDrunkTimeout(int beerPotency)
        {
            _drunkTimeoutSeconds = (100 + (beerPotency * 10)) / 8.0;
        }
    }

    public interface IPeriodicEmoteHandler : IGameComponent, IUpdateable
    {
        void SetDrunkTimeout(int beerPotency);
    }
}
