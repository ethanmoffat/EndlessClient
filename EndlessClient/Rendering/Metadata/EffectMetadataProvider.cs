using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Metadata.Models;

namespace EndlessClient.Rendering.Metadata
{
    [AutoMappedType(IsSingleton = true)]
    public class EffectMetadataProvider : IMetadataProvider<EffectMetadata>
    {
        public IReadOnlyDictionary<int, EffectMetadata> DefaultMetadata => _metadata;

        private readonly Dictionary<int, EffectMetadata> _metadata;
        private readonly IGFXMetadataLoader _metadataLoader;

        public EffectMetadataProvider(IGFXMetadataLoader metadataLoader)
        {
            // source: https://docs.google.com/spreadsheets/d/1DQgN4r2cH6HA2ydn4M6CpUJlClWXXBemosYP57k_o5I/edit#gid=0
            // todo: flickering effects are off-by-one. Fix metadata in the GFX (EndlessClient.Binaries), here, and in EffectSpriteInfo where a -1 is applied to the random choice
            _metadata = new Dictionary<int, EffectMetadata>
            {
                { 1, new EffectMetadata(true, true, true, SoundEffectID.PotionOfFlamesEffect, 4, 2, 0, 0, EffectAnimationType.Static, null, null, null) }, // small fire
                { 2, new EffectMetadata(false, false, true, SoundEffectID.PotionOfLoveEffect, 4, 4, 0, 0, EffectAnimationType.Static, null, null, null) }, // hearts
                { 3, new EffectMetadata(false, true, true, SoundEffectID.AdminWarp, 8, 1, 0, 0, EffectAnimationType.Static, null, null, null) }, // admin warp
                { 4, new EffectMetadata(false, false, true, SoundEffectID.AdminWarp, 8, 1, 0, 0, EffectAnimationType.Static, null, null, null) }, // admin warp 2
                { 5, new EffectMetadata(false, false, true, SoundEffectID.PotionOfFireworksEffect, 7, 2, 0, 0, EffectAnimationType.Static, null, null, null) }, // celebrate
                { 6, new EffectMetadata(false, true, true, SoundEffectID.PotionOfSparklesEffect, 5, 1, 0, 0, EffectAnimationType.Static, null, null, null) }, // schwing
                { 7, new EffectMetadata(true, false, false, SoundEffectID.PotionOfEvilTerrorEffect, 4, 4, 0, 0, EffectAnimationType.Static, null, null, null) }, // evil
                { 8, new EffectMetadata(true, false, false, SoundEffectID.PotionOfEvilTerrorEffect, 4, 4, 0, 0, EffectAnimationType.Static, null, null, null) }, // terror
                { 9, new EffectMetadata(true, false, false, SoundEffectID.Water, 6, 1, 0, 0, EffectAnimationType.Static, null, null, null) }, // water splash
                { 10, new EffectMetadata(false, true, true, SoundEffectID.Heal, 5, 1, 0, 0, EffectAnimationType.Static, null, null, null) }, // heal
                { 11, new EffectMetadata(false, false, true, SoundEffectID.Thunder, 4, 2, 0, 0, EffectAnimationType.Static, null, null, null) }, // small thunder
                { 12, new EffectMetadata(false, false, true, 0, 4, 8, 0, 0, EffectAnimationType.Static, null, null, null) }, // snow
                { 13, new EffectMetadata(true, true, false, SoundEffectID.UltimaBlastSpell, 4, 3, 0, 0, EffectAnimationType.Static, null, null, null) }, // ultima
                { 14, new EffectMetadata(true, true, false, SoundEffectID.PotionOfFlamesEffect, 6, 1, 0, -160, EffectAnimationType.VerticalSliding, new VerticalSlidingEffectMetadata(30), null, null) }, // fire ball
                { 15, new EffectMetadata(false, true, true, SoundEffectID.ShieldSpell, 6, 1, 0, 0, EffectAnimationType.Static, null, null, null) }, // shield
                { 16, new EffectMetadata(true, false, true, SoundEffectID.RingOfFireSpell, 4, 3, 0, 0, EffectAnimationType.Static, null, null, null) }, // ring of fire
                { 17, new EffectMetadata(false, true, true, SoundEffectID.IceBlastSpell1, 7, 1, 0, 0, EffectAnimationType.Static, null, null, null) }, // ice blast
                { 18, new EffectMetadata(false, false, true, SoundEffectID.EnergyBallSpell, 7, 1, 0, 0, EffectAnimationType.VerticalSliding, new VerticalSlidingEffectMetadata(-10), null, null) }, // energy ball
                { 19, new EffectMetadata(true, true, true, SoundEffectID.WhirlSpell, 4, 2, 0, -10, EffectAnimationType.Position, null, new PositionOffsetEffectMetadata(new[] { -20, 0, 20, 0 }, new[] { 0, 14, 0, -14 }), null) }, // whirl / tornado
                { 20, new EffectMetadata(false, true, false, SoundEffectID.AuraSpell, 5, 3, 0, -12, EffectAnimationType.Flickering, null, null, new RandomFlickeringEffectMetadata(3, 4)) }, // aura
                { 21, new EffectMetadata(false, false, true, SoundEffectID.BouldersSpell, 7, 1, 0, -160, EffectAnimationType.VerticalSliding, new VerticalSlidingEffectMetadata(30), null, null) }, // boulders
                { 22, new EffectMetadata(true, true, false, SoundEffectID.HeavenSpell, 5, 4, 0, -114, EffectAnimationType.Flickering, null, null, new RandomFlickeringEffectMetadata(3, 4)) }, // heaven
                { 23, new EffectMetadata(true, true, false, SoundEffectID.IceBlastSpell2, 6, 1, 0, -160, EffectAnimationType.VerticalSliding, new VerticalSlidingEffectMetadata(30), null, null) }, // blue flame
                { 24, new EffectMetadata(true, true, false, SoundEffectID.HeavenSpell, 5, 4, 0, -114, EffectAnimationType.Flickering, null, null, new RandomFlickeringEffectMetadata(3, 4)) }, // dark beam
                { 25, new EffectMetadata(false, false, true, SoundEffectID.AdminHide, 4, 2, 0, 0, EffectAnimationType.Static, null, null, null) }, // admin hide
                { 26, new EffectMetadata(true, true, false, SoundEffectID.DarkHandSpell, 5, 2, 0, 0, EffectAnimationType.Static, null, null, null) }, // dark hand
                { 27, new EffectMetadata(true, true, false, SoundEffectID.DarkHandSpell, 5, 2, 0, 0, EffectAnimationType.Static, null, null, null) }, // dark skull
                { 28, new EffectMetadata(false, false, true, SoundEffectID.FireBlastSpell, 4, 1, 0, 0, EffectAnimationType.Static, null, null, null) }, // fire blast
                { 29, new EffectMetadata(false, false, true, SoundEffectID.TentaclesSpell, 5, 2, 0, 0, EffectAnimationType.Static, null, null, null) }, // tentacles
                { 30, new EffectMetadata(true, false, true, SoundEffectID.PowerWindSpell, 6, 2, 0, 0, EffectAnimationType.Static, null, null, null) }, // power wind
                { 31, new EffectMetadata(true, false, true, SoundEffectID.MagicWhirlSpell, 15, 1, 0, 0, EffectAnimationType.Static, null, null, null) }, // magic whirl
                { 32, new EffectMetadata(true, true, false, SoundEffectID.DarkHandSpell, 6, 2, 0, 0, EffectAnimationType.Static, null, null, null) }, // dark bite
                { 33, new EffectMetadata(true, true, false, SoundEffectID.AuraSpell, 4, 4, 0, 0, EffectAnimationType.Static, null, null, null) }, // shell
                { 34, new EffectMetadata(true, true, false, SoundEffectID.EnergyBallSpell, 5, 1, 0, -44, EffectAnimationType.Static, null, null, null) } // green flame
            };
            _metadataLoader = metadataLoader;
        }

        public EffectMetadata GetValueOrDefault(int graphic)
        {
            return _metadataLoader.GetMetadata<EffectMetadata>(graphic)
                .ValueOr(DefaultMetadata.TryGetValue(graphic, out var ret) ? ret : EffectMetadata.Default);
        }
    }
}
