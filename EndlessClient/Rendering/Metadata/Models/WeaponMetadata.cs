using EndlessClient.Audio;

namespace EndlessClient.Rendering.Metadata.Models
{
    public record WeaponMetadata(int? Slash, SoundEffectID[] SFX, bool Ranged) : IGFXMetadata
    {
        public static WeaponMetadata Default { get; } = new WeaponMetadata(Slash: null, SFX: new[] { SoundEffectID.PunchAttack }, Ranged: false);
    }
}