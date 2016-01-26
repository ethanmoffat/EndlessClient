// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
	//todo: it would be cool to load this from a config file instead of having it hard-coded
	public class EffectSpriteManager
	{
		private enum HardCodedPotionEffect
		{
			FLAMES    = 0,
			LOVE      = 1,
			CELEBRATE = 4,
			SPARKLES  = 5,
			EVIL      = 6,
			TERROR    = 7
		}

		private enum HardCodedSpellGraphic
		{
			FIRE = 1,
			HEAL = 10,
			THUNDER = 11,
			ULTIMA_BLAST = 13,
			FIRE_BALL = 14,
			SHIELD = 15,
			RING_OF_FIRE = 16,
			ICE_BLAST_1 = 17,
			ENERGY_BALL = 18,
			WHIRL = 19,
			AURA = 20,
			BOULDER = 21,
			HEAVEN = 22,
			ICE_BLAST_2 = 23,
			DARK_BEAM = 24,
			DARK_HAND = 26,
			DARK_SKULL = 27,
			FIRE_BLAST = 28,
			TENTACLES = 29,
			POWER_WIND = 30,
			MAGIC_WHIRL = 31,
			DARK_BITE = 32,
			SHELL = 33,
			GREEN_FLAME = 34
		}

		private readonly INativeGraphicsManager _graphicsManager;

		public EffectSpriteManager(INativeGraphicsManager graphicsManager)
		{
			_graphicsManager = graphicsManager;
		}

		public IList<IEffectSpriteInfo> GetEffectInfo(EffectType effectType, int effectID)
		{
			switch (effectType)
			{
				case EffectType.Potion: return ResolvePotionEffect((HardCodedPotionEffect)effectID);
				case EffectType.Spell: return ResolveSpellEffect((HardCodedSpellGraphic)effectID);
				case EffectType.WarpOriginal:
				case EffectType.WarpDestination: return GetWarpEffect(effectType);
				case EffectType.WaterSplashies: return GetWaterEffect();
				default: throw new ArgumentOutOfRangeException("effectType", effectType, null);
			}
		}

		private IList<IEffectSpriteInfo> ResolvePotionEffect(HardCodedPotionEffect effect)
		{
			switch (effect)
			{
				case HardCodedPotionEffect.FLAMES:
					return new List<IEffectSpriteInfo>(3)
					{
						new EffectSpriteInfo(4, 2, false, 255, GetGraphic(101)),
						new EffectSpriteInfo(4, 2, true, 128, GetGraphic(102)),
						new EffectSpriteInfo(4, 2, true, 255, GetGraphic(103))
					};
				case HardCodedPotionEffect.LOVE:
					return new List<IEffectSpriteInfo>(1)
					{
						new EffectSpriteInfo(4, 4, true, 255, GetGraphic(106))
					};
				case HardCodedPotionEffect.CELEBRATE:
					return new List<IEffectSpriteInfo>(1)
					{
						new EffectSpriteInfo(7, 2, true, 255, GetGraphic(115))
					};
				case HardCodedPotionEffect.SPARKLES:
					return new List<IEffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(5, 1, true, 128, GetGraphic(117)),
						new EffectSpriteInfo(5, 1, true, 128, GetGraphic(118))
					};
				case HardCodedPotionEffect.EVIL:
					return new List<IEffectSpriteInfo>(1)
					{
						new EffectSpriteInfo(4, 4, false, 255, GetGraphic(119))
					};
				case HardCodedPotionEffect.TERROR:
					return new List<IEffectSpriteInfo>(1)
					{
						new EffectSpriteInfo(4, 4, false, 255, GetGraphic(122))
					};
				default: throw new ArgumentOutOfRangeException("effect", effect, null);
			}
		}

		private IList<IEffectSpriteInfo> ResolveSpellEffect(HardCodedSpellGraphic effect)
		{
			switch (effect)
			{
				case HardCodedSpellGraphic.FIRE: return ResolvePotionEffect(HardCodedPotionEffect.FLAMES);
				case HardCodedSpellGraphic.HEAL:
					return new List<IEffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(5, 1, false, 128, GetGraphic(129)),
						new EffectSpriteInfo(5, 1, true, 255, GetGraphic(130))
					};
				case HardCodedSpellGraphic.THUNDER:
					return new List<IEffectSpriteInfo>(1) { new EffectSpriteInfo(4, 1, true, 255, GetGraphic(133)) };
				case HardCodedSpellGraphic.ULTIMA_BLAST:
					return new List<IEffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(4, 3, true, 255, GetGraphic(137)),
						new EffectSpriteInfo(4, 3, true, 128, GetGraphic(138))
					};
				case HardCodedSpellGraphic.FIRE_BALL:
					return new List<IEffectSpriteInfo>(2)
					{
						new FallingEffectSpriteInfo(6, 1, false, 255, GetGraphic(140)),
						new FallingEffectSpriteInfo(6, 1, true, 128, GetGraphic(141))
					};
				case HardCodedSpellGraphic.SHIELD:
					return new List<IEffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(6, 1, false, 128, GetGraphic(144)),
						new EffectSpriteInfo(6, 1, true, 255, GetGraphic(145))
					};
				case HardCodedSpellGraphic.RING_OF_FIRE:
					return new List<IEffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(4, 3, false, 255, GetGraphic(146)),
						new EffectSpriteInfo(4, 3, true, 128, GetGraphic(148))
					};
				case HardCodedSpellGraphic.ICE_BLAST_1:
					return new List<IEffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(7, 1 ,false, 128, GetGraphic(150)),
						new EffectSpriteInfo(7, 1, true, 255, GetGraphic(151))
					};
				case HardCodedSpellGraphic.ENERGY_BALL:
					return new List<IEffectSpriteInfo>(1) { new EnergyBallEffectSpriteInfo(7, 1, true, 255, GetGraphic(154)) };
				case HardCodedSpellGraphic.WHIRL:
					return new List<IEffectSpriteInfo>(3) //todo: in the original client, this moves around more erractically
					{
						new EffectSpriteInfo(4, 2, false, 255, GetGraphic(155)),
						new EffectSpriteInfo(4, 2, true, 128, GetGraphic(156)),
						new EffectSpriteInfo(4, 2, true, 255, GetGraphic(155))
					};
				case HardCodedSpellGraphic.AURA:
					return new List<IEffectSpriteInfo>(1)
					{
						new AuraEffectSpriteInfo(GetGraphic(159))
					};
				case HardCodedSpellGraphic.BOULDER:
					return new List<IEffectSpriteInfo>(1)
					{
						new FallingEffectSpriteInfo(7, 1, true, 255, GetGraphic(163))
					};
				//case HardCodedSpellGraphic.HEAVEN:
				//	return new List<EffectSpriteInfo>(2)
				//	{
				//		//bottom is aligned with bottom of target
				//		//flashes between last 2 frames of graphic BEHIND FULLALPHA
				//		new HeavenEffectSpriteInfo(false, 255, GetGraphic(164), true),
				//		//flashes last frame of graphic ONTOP HALFALPHA
				//		new HeavenEffectSpriteInfo(true, 128, GetGraphic(165), false)
				//	};
			}

			//not implemented spell graphics will just not render anything
			return new IEffectSpriteInfo[] { };
		}

		private IList<IEffectSpriteInfo> GetWarpEffect(EffectType warpEffect)
		{
			int[] gfxIDs;
			switch (warpEffect)
			{
				case EffectType.WarpOriginal: gfxIDs = new[] {108, 109}; break;
				case EffectType.WarpDestination: gfxIDs = new[] {112}; break;
				default: throw new ArgumentOutOfRangeException("warpEffect", warpEffect, null);
			}

			return gfxIDs.Select(id => new EffectSpriteInfo(8, 1, true, 255, GetGraphic(id)))
						 .OfType<IEffectSpriteInfo>()
						 .ToList();
		}

		private IList<IEffectSpriteInfo> GetWaterEffect()
		{
			return new List<IEffectSpriteInfo>
			{
				new EffectSpriteInfo(6, 1, false, 255, GetGraphic(125))
			};
		}

		private Texture2D GetGraphic(int actualResourceID)
		{
			return _graphicsManager.TextureFromResource(GFXTypes.Spells, actualResourceID - 100, true);
		}
	}
}
