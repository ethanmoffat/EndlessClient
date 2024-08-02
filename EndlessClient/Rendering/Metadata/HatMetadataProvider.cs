using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Rendering.Metadata.Models;

namespace EndlessClient.Rendering.Metadata
{
    [AutoMappedType(IsSingleton = true)]
    public class HatMetadataProvider : IMetadataProvider<HatMetadata>
    {
        public IReadOnlyDictionary<int, HatMetadata> DefaultMetadata => _metadata;

        private readonly Dictionary<int, HatMetadata> _metadata;
        private readonly IGFXMetadataLoader _metadataLoader;

        public HatMetadataProvider(IGFXMetadataLoader metadataLoader)
        {
            _metadata = new Dictionary<int, HatMetadata>
            {
                {  7, new HatMetadata(HatMaskType.FaceMask) }, // bandana
                {  8, new HatMetadata(HatMaskType.FaceMask) }, // bandana
                {  9, new HatMetadata(HatMaskType.FaceMask) }, // bandana
                { 10, new HatMetadata(HatMaskType.FaceMask) }, // bandana
                { 11, new HatMetadata(HatMaskType.FaceMask) }, // bandana
                { 12, new HatMetadata(HatMaskType.FaceMask) }, // purple scarf
                { 13, new HatMetadata(HatMaskType.FaceMask) }, // red scarf
                { 14, new HatMetadata(HatMaskType.FaceMask) }, // black scarf
                { 15, new HatMetadata(HatMaskType.FaceMask) }, // dragon mask

                { 16, new HatMetadata(HatMaskType.HideHair) }, // black hood
                { 17, new HatMetadata(HatMaskType.HideHair) }, // brown hood
                { 18, new HatMetadata(HatMaskType.HideHair) }, // blue hood
                { 19, new HatMetadata(HatMaskType.HideHair) }, // green hood
                { 20, new HatMetadata(HatMaskType.HideHair) }, // red hood
                { 21, new HatMetadata(HatMaskType.HideHair) }, // chainmail hat

                { 25, new HatMetadata(HatMaskType.HideHair) }, // horned hat
                { 26, new HatMetadata(HatMaskType.HideHair) }, // merchant hat
                { 28, new HatMetadata(HatMaskType.HideHair) }, // helmy
                { 30, new HatMetadata(HatMaskType.HideHair) }, // eloff helmet
                { 31, new HatMetadata(HatMaskType.HideHair) }, // air hat

                { 32, new HatMetadata(HatMaskType.FaceMask) }, // frog head
                { 33, new HatMetadata(HatMaskType.FaceMask) }, // pilotte

                { 34, new HatMetadata(HatMaskType.HideHair) }, // beruta
                { 35, new HatMetadata(HatMaskType.HideHair) }, // pirate hat
                { 36, new HatMetadata(HatMaskType.HideHair) }, // lotus helmet
                { 37, new HatMetadata(HatMaskType.HideHair) }, // kitty hat
                { 38, new HatMetadata(HatMaskType.HideHair) }, // hula hula hat

                { 40, new HatMetadata(HatMaskType.HideHair) }, // gob helm
                { 41, new HatMetadata(HatMaskType.HideHair) }, // horned gob helm
                { 44, new HatMetadata(HatMaskType.HideHair) }, // helmet of darkness
                { 46, new HatMetadata(HatMaskType.HideHair) }, // flad hat
                { 47, new HatMetadata(HatMaskType.HideHair) }, // cook hat

                { 48, new HatMetadata(HatMaskType.FaceMask) }, // glasses
                { 50, new HatMetadata(HatMaskType.FaceMask) }, // medic cap
            };
            _metadataLoader = metadataLoader;
        }

        public HatMetadata GetValueOrDefault(int graphic)
        {
            return _metadataLoader.GetMetadata<HatMetadata>(graphic)
                .ValueOr(DefaultMetadata.TryGetValue(graphic, out var ret) ? ret : HatMetadata.Default);
        }
    }
}