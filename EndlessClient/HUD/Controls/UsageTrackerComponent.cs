// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.GameExecution;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD
{
	public class UsageTrackerComponent : GameComponent
	{
		private readonly IEndlessGameProvider _gameProvider;
		private readonly ICharacterRepository _characterRepository;
		private DateTime _lastUpdateTime;

		public UsageTrackerComponent(IEndlessGameProvider gameProvider,
									 ICharacterRepository characterRepository)
			: base((Game)gameProvider.Game)
		{
			_gameProvider = gameProvider;
			_characterRepository = characterRepository;
			_lastUpdateTime = DateTime.Now;
		}

		public override void Initialize()
		{
			if (!_gameProvider.Game.Components.Contains(this))
				_gameProvider.Game.Components.Add(this);

			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			if ((DateTime.Now - _lastUpdateTime).TotalMinutes >= 1)
			{
				var stats = _characterRepository.ActiveCharacter.Stats;
				stats = stats.WithNewStat(CharacterStat.Usage, stats.Stats[CharacterStat.Usage] + 1);
				_characterRepository.ActiveCharacter = _characterRepository.ActiveCharacter
																	   .WithStats(stats);
				_lastUpdateTime = DateTime.Now;
			}

			base.Update(gameTime);
		}
	}
}
