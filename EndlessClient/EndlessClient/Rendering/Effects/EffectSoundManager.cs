// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.Audio;
using Microsoft.Xna.Framework.Audio;

namespace EndlessClient.Rendering.Effects
{
	public class EffectSoundManager
	{
		private readonly SoundManager _soundManager;

		public EffectSoundManager(SoundManager soundManager)
		{
			_soundManager = soundManager;
		}

		public IList<SoundEffectInstance> GetSoundEffectsForEffect(EffectType type, int id)
		{
			switch (type)
			{
				case EffectType.Potion: return GetPotionSoundEffect(id);
				case EffectType.Spell: return GetSpellSoundEffect(id);
				case EffectType.WarpOriginal:
				case EffectType.WarpDestination: return GetWarpSoundEffect(type);
				case EffectType.WaterSplashies: return GetWaterSoundEffect();
				default: throw new ArgumentOutOfRangeException("type", type, null);
			}
		}

		private IList<SoundEffectInstance> GetPotionSoundEffect(int id)
		{
			return new SoundEffectInstance[0];
		}

		private IList<SoundEffectInstance> GetSpellSoundEffect(int id)
		{
			return new SoundEffectInstance[0];
		}

		private IList<SoundEffectInstance> GetWarpSoundEffect(EffectType type)
		{
			return new SoundEffectInstance[0];
		}

		private IList<SoundEffectInstance> GetWaterSoundEffect()
		{
			return new[] { _soundManager.GetSoundEffectRef(SoundEffectID.Water) };
		}
	}
}
