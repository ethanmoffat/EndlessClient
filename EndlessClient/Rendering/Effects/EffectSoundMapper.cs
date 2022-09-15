using System;
using System.Collections.Generic;
using EndlessClient.Audio;

namespace EndlessClient.Rendering.Effects
{
    public class EffectSoundMapper
    {
        public IEnumerable<SoundEffectID> GetSoundEffectsForEffect(EffectType type, int id)
        {
            switch (type)
            {
                case EffectType.Potion: return GetPotionSoundEffect(id);
                case EffectType.Spell: return GetSpellSoundEffect(id);
                case EffectType.WarpOriginal:
                case EffectType.WarpDestination: return GetWarpSoundEffect();
                case EffectType.WaterSplashies: return GetWaterSoundEffect();
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static IEnumerable<SoundEffectID> GetPotionSoundEffect(int id)
        {
            switch ((HardCodedPotionEffect)id)
            {
                case HardCodedPotionEffect.FLAMES: yield return SoundEffectID.PotionOfFlamesEffect; break;
                case HardCodedPotionEffect.LOVE: yield return SoundEffectID.LearnNewSpell; break;
                case HardCodedPotionEffect.CELEBRATE: yield return SoundEffectID.PotionOfFireworksEffect; break;
                case HardCodedPotionEffect.SPARKLES: yield return SoundEffectID.PotionOfSparklesEffect; break;
                case HardCodedPotionEffect.EVIL:
                case HardCodedPotionEffect.TERROR: yield return SoundEffectID.PotionOfEvilTerrorEffect; break;
                default:
                    {
                        foreach (var effect in GetSpellSoundEffect(id + 1))
                            yield return effect;
                    }
                    break;
            }
        }

        private static IEnumerable<SoundEffectID> GetSpellSoundEffect(int id)
        {
            switch (id) // id is GFX; not pub file id
            {
                case 1: // small fire / fire from the sky
                case 14: yield return SoundEffectID.PotionOfFlamesEffect; break;
                case 10: yield return SoundEffectID.Heal; break; // various heals
                case 11: yield return SoundEffectID.Thunder; break;
                case 13: yield return SoundEffectID.UltimaBlastSpell; break;
                case 15: yield return SoundEffectID.ShieldSpell; break; // magic/attack shield
                case 16: yield return SoundEffectID.RingOfFireSpell; break;
                case 17: yield return SoundEffectID.IceBlastSpell1; break;
                case 18: // energy ball / green flame
                case 34: yield return SoundEffectID.EnergyBallSpell; break;
                case 19: yield return SoundEffectID.WhirlSpell; break;
                case 20: // aura/shell
                case 33: yield return SoundEffectID.AuraSpell; break;
                case 21: yield return SoundEffectID.BouldersSpell; break;
                case 22: // heaven/dark beam
                case 24: yield return SoundEffectID.HeavenSpell; break;
                case 23: yield return SoundEffectID.IceBlastSpell2; break;
                case 26: // dark hand/dark skull/dark bite
                case 27:
                case 32: yield return SoundEffectID.DarkHandSpell; break;
                case 28: yield return SoundEffectID.FireBlastSpell; break;
                case 29: yield return SoundEffectID.TentaclesSpell; break;
                case 30: yield return SoundEffectID.PowerWindSpell; break;
                case 31: yield return SoundEffectID.MagicWhirlSpell; break;
            }
        }

        private static IEnumerable<SoundEffectID> GetWarpSoundEffect()
        {
            yield return SoundEffectID.AdminWarp;
        }

        private static IEnumerable<SoundEffectID> GetWaterSoundEffect()
        {
            yield return SoundEffectID.Water;
        }
    }
}
