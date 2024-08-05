namespace EndlessClient.Rendering.Metadata.Models
{
    public enum HatMaskType
    {
        Standard,
        FaceMask,
        HideHair
    }

    public record HatMetadata(HatMaskType ClipMode) : IGFXMetadata
    {
        public static HatMetadata Default { get; } = new HatMetadata(HatMaskType.Standard);
    }
}