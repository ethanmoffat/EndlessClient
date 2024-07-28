using AutomaticTypeMapper;
using EndlessClient.Rendering.Metadata.Models;
using System.Collections.Generic;

namespace EndlessClient.Rendering.Metadata
{
    [AutoMappedType(IsSingleton = true)]
    public class ShieldMetadataProvider : IMetadataProvider<ShieldMetadata>
    {
        public IReadOnlyDictionary<int, ShieldMetadata> DefaultMetadata => _metadata;

        private readonly Dictionary<int, ShieldMetadata> _metadata;
        private readonly IGFXMetadataLoader _metadataLoader;

        public ShieldMetadataProvider(IGFXMetadataLoader metadataLoader)
        {
            _metadata = new Dictionary<int, ShieldMetadata>
            {
                { 10, new ShieldMetadata(true) }, // good wings
                { 11, new ShieldMetadata(true) }, // bag
                { 14, new ShieldMetadata(true) }, // normal arrows
                { 15, new ShieldMetadata(true) }, // frost arrows
                { 16, new ShieldMetadata(true) }, // fire arrows
                { 18, new ShieldMetadata(true) }, // good force wings
                { 19, new ShieldMetadata(true) }, // fire force wings
            };
            _metadataLoader = metadataLoader;
        }

        public ShieldMetadata GetValueOrDefault(int graphic)
        {
            return _metadataLoader.GetMetadata<ShieldMetadata>(graphic)
                .ValueOr(DefaultMetadata.TryGetValue(graphic, out var ret) ? ret : ShieldMetadata.Default);
        }
    }
}