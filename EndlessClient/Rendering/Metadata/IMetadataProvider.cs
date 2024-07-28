using EndlessClient.Rendering.Metadata.Models;
using System.Collections.Generic;

namespace EndlessClient.Rendering.Metadata;

public interface IMetadataProvider<TMetadata>
    where TMetadata : IGFXMetadata
{
    IReadOnlyDictionary<int, TMetadata> DefaultMetadata { get; }

    TMetadata GetValueOrDefault(int graphic);
}