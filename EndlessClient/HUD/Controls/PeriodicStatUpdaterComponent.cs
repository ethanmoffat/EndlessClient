using System;
using System.Diagnostics;
using EndlessClient.GameExecution;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Optional;

namespace EndlessClient.HUD.Controls
{
    public class PeriodicStatUpdaterComponent : GameComponent
    {
        private readonly IEndlessGameProvider _gameProvider;
        private readonly ICharacterRepository _characterRepository;

        private DateTime _lastUsageUpdateTime;
        private Option<Stopwatch> _lastSpUpdateTime;

        public PeriodicStatUpdaterComponent(IEndlessGameProvider gameProvider,
                                            ICharacterRepository characterRepository)
            : base((Game)gameProvider.Game)
        {
            _gameProvider = gameProvider;
            _characterRepository = characterRepository;
            _lastUsageUpdateTime = DateTime.Now;
            _lastSpUpdateTime = Option.None<Stopwatch>();
        }

        public override void Initialize()
        {
            if (!_gameProvider.Game.Components.Contains(this))
                _gameProvider.Game.Components.Add(this);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            var stats = _characterRepository.MainCharacter.Stats;

            if ((DateTime.Now - _lastUsageUpdateTime).TotalMinutes >= 1)
            {
                stats = stats.WithNewStat(CharacterStat.Usage, stats.Stats[CharacterStat.Usage] + 1);
                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
                _lastUsageUpdateTime = DateTime.Now;
            }

            if (stats[CharacterStat.SP] < stats[CharacterStat.MaxSP] - 1)
            {
                _lastSpUpdateTime.Match(
                    some: t =>
                    {
                        // this seems close to 2 but is probably based on level and/or one of the stats
                        if (t.ElapsedMilliseconds > 2000 - Math.Max(800, _characterRepository.MainCharacter.Stats[CharacterStat.Level] * 80))
                        {
                            var spUpdate = _characterRepository.MainCharacter.RenderProperties.SitState != SitState.Standing ? 4 : 2;
                            var sp = Math.Min(stats[CharacterStat.SP] + spUpdate, stats[CharacterStat.MaxSP]);
                            stats = stats.WithNewStat(CharacterStat.SP, sp);
                            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

                            if (stats[CharacterStat.SP] == stats[CharacterStat.MaxSP])
                                _lastSpUpdateTime = Option.None<Stopwatch>();
                            else
                                _lastSpUpdateTime = Option.Some(Stopwatch.StartNew());
                        }
                    },
                    none: () => _lastSpUpdateTime = Option.Some(Stopwatch.StartNew()));
            }

            base.Update(gameTime);
        }
    }
}