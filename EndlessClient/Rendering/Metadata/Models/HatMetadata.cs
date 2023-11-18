using Amadevus.RecordGenerator;

namespace EndlessClient.Rendering.Metadata.Models;

public enum HatMaskType
{
    Standard,
    FaceMask,
    HideHair
}

[Record]
public sealed record HatMetadata(HatMaskType ClipMode) : IGFXMetadata;
