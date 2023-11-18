namespace EndlessClient.Rendering.Metadata.Models;

public sealed record ShieldMetadata(bool IsShieldOnBack) : IGFXMetadata
{
    public static ShieldMetadata Default { get; } = new ShieldMetadata(false);
}
