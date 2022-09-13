using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
    //todo: it would be cool to load this from a config file instead of having it hard-coded
    public partial class EffectSpriteManager
    {
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

        private Dictionary<HardCodedPotionEffect, IList<IEffectSpriteInfo>> _potionEffects;
        private Dictionary<HardCodedSpellGraphic, IList<IEffectSpriteInfo>> _spellEffects;

        private readonly INativeGraphicsManager _graphicsManager;

        public EffectSpriteManager(INativeGraphicsManager graphicsManager)
        {
            _graphicsManager = graphicsManager;
            CreatePotionEffectDictionary();
            CreateSpellEffectDictionary();
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
                default: throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null);
            }
        }

        private IList<IEffectSpriteInfo> ResolvePotionEffect(HardCodedPotionEffect effect)
        {
            if (_potionEffects.ContainsKey(effect))
            {
                var retList = _potionEffects[effect];
                foreach (var item in retList)
                    item.Restart();
            }

            // potion graphics that aren't in the hard-coded list here use the same formula as spell graphics
            return ResolveSpellEffect((HardCodedSpellGraphic)effect+1);
        }

        private IList<IEffectSpriteInfo> ResolveSpellEffect(HardCodedSpellGraphic effect)
        {
            if (_spellEffects.ContainsKey(effect))
            {
                var retList = _spellEffects[effect];
                foreach (var item in retList)
                    item.Restart();
                return new List<IEffectSpriteInfo>(retList);
            }

            // not implemented spell graphics have a default rendering set
            // spell effects seem to start at GFX 128 and go in sets of 3, indexed on (spellGraphic - 10)
            // first gfx is behind character, other 2 are in front
            // 255 alpha assumed until proven otherwise
            // 4 frames assumed until proven otherwise
            return new List<IEffectSpriteInfo>
            {
                new CustomEffectSpriteInfo(4, 2, false, 255, GetGraphic(((int)effect - 9)*3 + 128)),
                new CustomEffectSpriteInfo(4, 2, true, 255, GetGraphic(((int)effect - 9)*3 + 129)),
                new CustomEffectSpriteInfo(4, 2, true, 255, GetGraphic(((int)effect - 9)*3 + 130))
            };
        }

        private IList<IEffectSpriteInfo> GetWarpEffect(EffectType warpEffect)
        {
            int[] gfxIDs;
            switch (warpEffect)
            {
                case EffectType.WarpOriginal: gfxIDs = new[] {108, 109}; break;
                case EffectType.WarpDestination: gfxIDs = new[] {112}; break;
                default: throw new ArgumentOutOfRangeException(nameof(warpEffect), warpEffect, null);
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

        private void CreatePotionEffectDictionary()
        {
            _potionEffects = new Dictionary<HardCodedPotionEffect, IList<IEffectSpriteInfo>>(6)
            {
                {
                    HardCodedPotionEffect.FLAMES,
                    CreateList(new EffectSpriteInfo(4, 2, false, 255, GetGraphic(101)),
                               new EffectSpriteInfo(4, 2, true, 128, GetGraphic(102)),
                               new EffectSpriteInfo(4, 2, true, 255, GetGraphic(103)))
                },
                {
                    HardCodedPotionEffect.LOVE,
                    CreateList(new EffectSpriteInfo(4, 4, true, 255, GetGraphic(106)))
                },
                {
                    HardCodedPotionEffect.CELEBRATE,
                    CreateList(new EffectSpriteInfo(7, 2, true, 255, GetGraphic(115)))
                },
                {
                    HardCodedPotionEffect.SPARKLES,
                    CreateList(new EffectSpriteInfo(5, 1, true, 128, GetGraphic(117)),
                               new EffectSpriteInfo(5, 1, true, 128, GetGraphic(118)))
                },
                {
                    HardCodedPotionEffect.EVIL,
                    CreateList(new EffectSpriteInfo(4, 4, false, 255, GetGraphic(119)))
                },
                {
                    HardCodedPotionEffect.TERROR, 
                    CreateList(new EffectSpriteInfo(4, 4, false, 255, GetGraphic(122)))
                }
            };
        }

        private void CreateSpellEffectDictionary()
        {
            _spellEffects = new Dictionary<HardCodedSpellGraphic, IList<IEffectSpriteInfo>>(32)
            {
                { HardCodedSpellGraphic.FIRE, _potionEffects[HardCodedPotionEffect.FLAMES] },
                { HardCodedSpellGraphic.HEAL, 
                    CreateList(new EffectSpriteInfo(5, 1, false, 128, GetGraphic(129)),
                               new EffectSpriteInfo(5, 1, true, 255, GetGraphic(130)))
                },
                {
                    HardCodedSpellGraphic.THUNDER,
                    CreateList(new EffectSpriteInfo(4, 1, true, 255, GetGraphic(133)))
                },
                {
                    HardCodedSpellGraphic.ULTIMA_BLAST,
                    CreateList(new EffectSpriteInfo(4, 3, true, 255, GetGraphic(137)),
                               new EffectSpriteInfo(4, 3, true, 128, GetGraphic(138)))
                },
                {
                    HardCodedSpellGraphic.FIRE_BALL,
                    CreateList(new FallingEffectSpriteInfo(6, 1, false, 255, GetGraphic(140)),
                               new FallingEffectSpriteInfo(6, 1, true, 128, GetGraphic(141)))
                },
                {
                    HardCodedSpellGraphic.SHIELD,
                    CreateList(new EffectSpriteInfo(6, 1, false, 128, GetGraphic(144)),
                               new EffectSpriteInfo(6, 1, true, 255, GetGraphic(145)))
                },
                {
                    HardCodedSpellGraphic.RING_OF_FIRE,
                    CreateList(new EffectSpriteInfo(4, 3, false, 255, GetGraphic(146)),
                               new EffectSpriteInfo(4, 3, true, 128, GetGraphic(148)))
                },
                {
                    HardCodedSpellGraphic.ICE_BLAST_1,
                    CreateList(new EffectSpriteInfo(7, 1 ,false, 128, GetGraphic(150)),
                               new EffectSpriteInfo(7, 1, true, 255, GetGraphic(151)))
                },
                {
                    HardCodedSpellGraphic.ENERGY_BALL,
                    CreateList(new EnergyBallEffectSpriteInfo(7, 1, true, 255, GetGraphic(154)))
                },
                {
                    HardCodedSpellGraphic.WHIRL,
                    CreateList(new EffectSpriteInfo(4, 2, false, 255, GetGraphic(155)),
                               new EffectSpriteInfo(4, 2, true, 128, GetGraphic(156)),
                               new EffectSpriteInfo(4, 2, true, 255, GetGraphic(157)))
                },
                {
                    HardCodedSpellGraphic.AURA,
                    CreateList(new AuraEffectSpriteInfo(GetGraphic(159)))
                },
                {
                    HardCodedSpellGraphic.BOULDER,
                    CreateList(new FallingEffectSpriteInfo(7, 1, true, 255, GetGraphic(163)))
                },
                //{
                //    HardCodedSpellGraphic.HEAVEN, //todo: this isn't quite right yet...
                //    CreateList(new HeavenEffectSpriteInfo(false, 255, GetGraphic(164)),
                //               new HeavenEffectSpriteInfo(true, 128, GetGraphic(165)))
                //},
                {
                    HardCodedSpellGraphic.ICE_BLAST_2,
                    CreateList(new FallingEffectSpriteInfo(6, 1, false, 255, GetGraphic(167)),
                               new FallingEffectSpriteInfo(6, 1, true, 255, GetGraphic(168)))
                },
                //{
                //    HardCodedSpellGraphic.DARK_BEAM, //todo: figure this out...
                //}
                {
                    HardCodedSpellGraphic.DARK_HAND,
                    CreateList(new EffectSpriteInfo(5, 2, false, 255, GetGraphic(176)),
                               new EffectSpriteInfo(5, 2, true, 128, GetGraphic(177)))
                },
                {
                    HardCodedSpellGraphic.DARK_SKULL,
                    CreateList(new EffectSpriteInfo(5, 2, false, 255, GetGraphic(179)),
                               new EffectSpriteInfo(5, 2, true, 128, GetGraphic(180)))
                },
                {
                    HardCodedSpellGraphic.FIRE_BLAST,
                    CreateList(new EffectSpriteInfo(4, 1, true, 192, GetGraphic(184)))
                },
                {
                    HardCodedSpellGraphic.TENTACLES,
                    CreateList(new EffectSpriteInfo(5, 2, false, 255, GetGraphic(185)),
                               new EffectSpriteInfo(5, 2, true, 255, GetGraphic(187)))
                },
                {
                    HardCodedSpellGraphic.POWER_WIND,
                    CreateList(new EffectSpriteInfo(6, 2, false, 255, GetGraphic(188)),
                               new EffectSpriteInfo(6, 2, true, 255, GetGraphic(190)))
                },
                {
                    HardCodedSpellGraphic.MAGIC_WHIRL,
                    CreateList(new BottomAlignedEffectSpriteInfo(15, 1, false, 255, GetGraphic(191)),
                               new BottomAlignedEffectSpriteInfo(15, 1, true, 128, GetGraphic(193)))
                },
                {
                    HardCodedSpellGraphic.DARK_BITE,
                    CreateList(new EffectSpriteInfo(6, 2, false, 255, GetGraphic(194)),
                               new EffectSpriteInfo(6, 2, true, 128, GetGraphic(195)))
                },
                {
                    HardCodedSpellGraphic.SHELL, //todo: why isn't this rendering correctly?
                    CreateList(new EffectSpriteInfo(4, 4, false, 255, GetGraphic(197)),
                               new EffectSpriteInfo(4, 4, true, 128, GetGraphic(198)))
                },
                {
                    HardCodedSpellGraphic.GREEN_FLAME,
                    CreateList(new BottomAlignedEffectSpriteInfo(5, 1, false, 255, GetGraphic(200)),
                               new BottomAlignedEffectSpriteInfo(5, 1, true, 128, GetGraphic(201)))
                }
            };
        }

        private IList<IEffectSpriteInfo> CreateList(params IEffectSpriteInfo[] effects)
        {
            return effects.ToList();
        }
    }
}
