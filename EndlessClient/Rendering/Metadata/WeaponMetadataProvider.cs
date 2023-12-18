using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Rendering.Metadata.Models;
using System.Collections.Generic;

namespace EndlessClient.Rendering.Metadata
{
    [AutoMappedType(IsSingleton = true)]
    public class WeaponMetadataProvider : IMetadataProvider<WeaponMetadata>
    {
        public IReadOnlyDictionary<int, WeaponMetadata> DefaultMetadata => _metadata;

        private readonly Dictionary<int, WeaponMetadata> _metadata;
        private readonly IGFXMetadataLoader _metadataLoader;

        public WeaponMetadataProvider(IGFXMetadataLoader metadataLoader)
        {
            _metadata = new Dictionary<int, WeaponMetadata>
            {
                { 0, new WeaponMetadata( null, new[] { SoundEffectID.PunchAttack }, false) }, // fist
                { 1, new WeaponMetadata( 3, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // wood axe
                { 2, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // sai
                { 3, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // dragon blade
                { 4, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // dagger
                { 5, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // spear
                { 6, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // saber
                { 7, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // staff
                { 8, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // book
                { 9, new WeaponMetadata( 3, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // mace
                { 10, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // spirit star
                { 11, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // throw axe
                { 12, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // dark katana
                { 13, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // short sword
                { 14, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // broadsword
                { 15, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // broom
                { 16, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // ninchackus
                { 17, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // ancient star
                { 18, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // battle axe
                { 19, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // ancient sword
                { 20, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // luna staff
                { 21, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // lance
                { 22, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // aura staff
                { 23, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // forest staff
                { 24, new WeaponMetadata( 1, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // normal sword
                { 25, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // jewel staff
                { 26, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // thor's hammer
                { 27, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // light katana
                { 28, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // polearm
                { 29, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // sickle
                { 30, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // trident
                { 31, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // warlock sword
                { 32, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // whip
                { 33, new WeaponMetadata( 5, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // ultima
                { 34, new WeaponMetadata( 5, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // ice blade
                { 35, new WeaponMetadata( 1, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // gold defender
                { 36, new WeaponMetadata( 4, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // lotus sword
                { 37, new WeaponMetadata( 4, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // cristal sword
                { 38, new WeaponMetadata( 5, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // killing edge
                { 39, new WeaponMetadata( 7, new[] { SoundEffectID.AlternateMeleeAttack }, false) }, // dark blade
                { 40, new WeaponMetadata( 7, new[] { SoundEffectID.AlternateMeleeAttack }, false) }, // reaper scyth
                { 41, new WeaponMetadata( 1, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // crescent staff
                { 42, new WeaponMetadata( 0, new[] { SoundEffectID.AttackBow }, true) }, // bow
                { 43, new WeaponMetadata( 0, new[] { SoundEffectID.AttackBow }, true) }, // xbow
                { 44, new WeaponMetadata( 8, new[] { SoundEffectID.AlternateMeleeAttack }, false) }, // reaper
                { 45, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // hockey stick
                { 46, new WeaponMetadata( 5, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // twin blades
                { 47, new WeaponMetadata( 1, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // lefor mace
                { 48, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // cava staff
                { 49, new WeaponMetadata( 0, new[] { SoundEffectID.Harp1,SoundEffectID.Harp2,SoundEffectID.Harp3}, true) }, // harp
                { 50, new WeaponMetadata( 0, new[] { SoundEffectID.Guitar1,SoundEffectID.Guitar2, SoundEffectID.Guitar3 }, true) }, // guitar
                { 51, new WeaponMetadata( 5, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // battle spear
                { 52, new WeaponMetadata( 1, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // flail
                { 53, new WeaponMetadata( 1, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // war axe
                { 54, new WeaponMetadata( 1, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // gastro
                { 55, new WeaponMetadata( 7, new[] { SoundEffectID.AlternateMeleeAttack }, false) }, // ablo staff
                { 56, new WeaponMetadata( 1, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // fluon sword
                { 57, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // rapier
                { 58, new WeaponMetadata( 0, new[] { SoundEffectID.Gun }, true) }, // gun
                { 59, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // knob staff
                { 60, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // fladdat staff
                { 61, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // gabrasto
                { 62, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // battle spear 2
                { 63, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // lens of truth
                { 64, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // chopper
                { 65, new WeaponMetadata( 3, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // adger
                { 66, new WeaponMetadata( 1, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // chains
                { 67, new WeaponMetadata( 2, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // mitova
                { 68, new WeaponMetadata( 3, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // merhawk
                { 69, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // kontra
                { 70, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // jack spear
                { 71, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // bazar staff
                { 72, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // saw blade
                { 73, new WeaponMetadata( 0, new[] { SoundEffectID.AttackBow }, true) }, // scav bow
                { 74, new WeaponMetadata( 0, new[] { SoundEffectID.MeleeWeaponAttack }, false) }, // fan
            };
            _metadataLoader = metadataLoader;
        }

        public WeaponMetadata GetValueOrDefault(int graphic)
        {
            return _metadataLoader.GetMetadata<WeaponMetadata>(graphic)
                .ValueOr(DefaultMetadata.TryGetValue(graphic, out var ret) ? ret : WeaponMetadata.Default);
        }
    }
}
