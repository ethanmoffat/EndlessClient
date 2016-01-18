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

		private readonly INativeGraphicsManager _graphicsManager;

		public EffectSpriteManager(INativeGraphicsManager graphicsManager)
		{
			_graphicsManager = graphicsManager;
		}

		public IList<EffectSpriteInfo> GetEffectInfo(EffectType effectType, int effectID)
		{
			switch (effectType)
			{
				case EffectType.Potion: return ResolvePotionEffect((HardCodedPotionEffect)effectID);
				case EffectType.Spell: return ResolveSpellEffect(effectID);
				case EffectType.WarpOriginal:
				case EffectType.WarpDestination: return GetWarpEffect(effectType);
				case EffectType.WaterSplashies: return GetWaterEffect();
				default: throw new ArgumentOutOfRangeException("effectType", effectType, null);
			}
		}

		private IList<EffectSpriteInfo> ResolvePotionEffect(HardCodedPotionEffect effect)
		{
			//FIRE: Effect #0 - 4 frames (same for small fire spell)
			// 101 - behind character
			// 102 - around character, 50% opacity
			// 103 - in front of character
			// repeats x2

			//LOVE: Effect #1 - 4 frames
			// 106 - in front of character
			// repeats x4

			//CELEBRATE: Effect #4 - 7 frames
			// 115 - in front of character
			// repeats x2

			//SPARKLES: Effect #5 - 5 frames (this one is weird)
			// 117 - in front of character (possible opacity?)
			// 118 - in front of character
			// repeats x1

			//EVIL: Effect #6 - 4 frames
			// 119 - behind character
			// repeats x4
			
			//TERROR: Effect #7 - 4 frames
			// 122 - behind character
			// repeats x4

			switch (effect)
			{
				case HardCodedPotionEffect.FLAMES:
					return new List<EffectSpriteInfo>(3)
					{
						new EffectSpriteInfo(4, 2, false, 255, GetGraphic(101)),
						new EffectSpriteInfo(4, 2, true, 128, GetGraphic(102)),
						new EffectSpriteInfo(4, 2, true, 255, GetGraphic(103))
					};
				case HardCodedPotionEffect.LOVE:
					return new List<EffectSpriteInfo>(1)
					{
						new EffectSpriteInfo(4, 4, true, 255, GetGraphic(106))
					};
				case HardCodedPotionEffect.CELEBRATE:
					return new List<EffectSpriteInfo>(1)
					{
						new EffectSpriteInfo(7, 2, true, 255, GetGraphic(115))
					};
				case HardCodedPotionEffect.SPARKLES:
					return new List<EffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(5, 1, true, 255, GetGraphic(117)),
						new EffectSpriteInfo(5, 1, true, 255, GetGraphic(118))
					};
				case HardCodedPotionEffect.EVIL:
					return new List<EffectSpriteInfo>(1)
					{
						new EffectSpriteInfo(4, 4, false, 255, GetGraphic(119))
					};
				case HardCodedPotionEffect.TERROR:
					return new List<EffectSpriteInfo>(1)
					{
						new EffectSpriteInfo(4, 4, false, 255, GetGraphic(122))
					};
				default: throw new ArgumentOutOfRangeException("effect", effect, null);
			}
		}

		private IList<EffectSpriteInfo> ResolveSpellEffect(int effectID)
		{
			switch (effectID)
			{
				case 1: return ResolvePotionEffect(HardCodedPotionEffect.FLAMES);
				case 10: //Heal spells
					return new List<EffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(5, 1, false, 128, GetGraphic(129)),
						new EffectSpriteInfo(5, 1, true, 255, GetGraphic(130))
					};
				case 11: //Small Thunder
					return new List<EffectSpriteInfo>(1) { new EffectSpriteInfo(4, 1, true, 255, GetGraphic(133)) };
				case 13: //Ultima Blast
					return new List<EffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(4, 3, true, 255, GetGraphic(137)),
						new EffectSpriteInfo(4, 3, true, 128, GetGraphic(138))
					};
				case 14: //Fire ball
					return new List<EffectSpriteInfo>(2)
					{
						new FireballEffectSpriteInfo(6, 1, false, 255, GetGraphic(140)),
						new FireballEffectSpriteInfo(6, 1, true, 128, GetGraphic(141))
					};
				case 15: //Magic and Attack Shield
					return new List<EffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(6, 1, false, 128, GetGraphic(144)),
						new EffectSpriteInfo(6, 1, true, 255, GetGraphic(145))
					};
				case 16: //Ring of Fire
					return new List<EffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(4, 3, false, 255, GetGraphic(146)),
						new EffectSpriteInfo(4, 3, true, 128, GetGraphic(148))
					};
				case 17: //Ice Blast (1)
					return new List<EffectSpriteInfo>(2)
					{
						new EffectSpriteInfo(7, 1 ,false, 128, GetGraphic(150)),
						new EffectSpriteInfo(7, 1, true, 255, GetGraphic(151))
					};
				case 18: //Energy Ball
					return new List<EffectSpriteInfo>(1) { new EnergyBallEffectSpriteInfo(7, 1, true, 255, GetGraphic(154)) };
				case 19: //hWhirl
					return new List<EffectSpriteInfo>(3) //todo: in the original client, this moves around more erractically
					{
						new EffectSpriteInfo(4, 2, false, 255, GetGraphic(155)),
						new EffectSpriteInfo(4, 2, true, 128, GetGraphic(156)),
						new EffectSpriteInfo(4, 2, true, 255, GetGraphic(155))
					};
			}

			//not implemented spell graphics will just not render anything
			return new EffectSpriteInfo[] {};
		}

		private IList<EffectSpriteInfo> GetWarpEffect(EffectType warpEffect)
		{
			//old position: 108/109 (character gone)
			//new position: 112 only
			//all:
			// in front of character
			// repeats once
			// 100% opacity
			// 8 frames

			int[] gfxIDs;
			switch (warpEffect)
			{
				case EffectType.WarpOriginal: gfxIDs = new[] {108, 109}; break;
				case EffectType.WarpDestination: gfxIDs = new[] {112}; break;
				default: throw new ArgumentOutOfRangeException("warpEffect", warpEffect, null);
			}

			return gfxIDs.Select(id => new EffectSpriteInfo(8, 1, true, 255, GetGraphic(id))).ToList();
		}

		private IList<EffectSpriteInfo> GetWaterEffect()
		{
			return new List<EffectSpriteInfo>
			{
				new EffectSpriteInfo(6, 1, false, 255, GetGraphic(125))
			};
		}

		private Texture2D GetGraphic(int gfx)
		{
			return _graphicsManager.TextureFromResource(GFXTypes.Spells, gfx - 100, true);
		}
	}
}
