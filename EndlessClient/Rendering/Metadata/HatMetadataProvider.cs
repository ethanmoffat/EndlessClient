using AutomaticTypeMapper;
using EndlessClient.Rendering.Metadata.Models;
using System.Collections.Generic;

namespace EndlessClient.Rendering.Metadata
{
    public interface IHatMetadataProvider
    {
        IReadOnlyDictionary<int, HatMetadata> DefaultMetadata { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class HatMetadataProvider : IHatMetadataProvider
    {
        public IReadOnlyDictionary<int, HatMetadata> DefaultMetadata => _metadata;

        private readonly Dictionary<int, HatMetadata> _metadata;

        public HatMetadataProvider()
        {
            _metadata = new Dictionary<int, HatMetadata>
            {
                {  7, new HatMetadata(Content.HatMaskType.FaceMask) }, // bandana
                {  8, new HatMetadata(Content.HatMaskType.FaceMask) }, // bandana
                {  9, new HatMetadata(Content.HatMaskType.FaceMask) }, // bandana
                { 10, new HatMetadata(Content.HatMaskType.FaceMask) }, // bandana
                { 11, new HatMetadata(Content.HatMaskType.FaceMask) }, // bandana
                { 12, new HatMetadata(Content.HatMaskType.FaceMask) }, // purple scarf
                { 13, new HatMetadata(Content.HatMaskType.FaceMask) }, // red scarf
                { 14, new HatMetadata(Content.HatMaskType.FaceMask) }, // black scarf
                { 15, new HatMetadata(Content.HatMaskType.FaceMask) }, // dragon mask

                { 16, new HatMetadata(Content.HatMaskType.HideHair) }, // black hood
                { 17, new HatMetadata(Content.HatMaskType.HideHair) }, // brown hood
                { 18, new HatMetadata(Content.HatMaskType.HideHair) }, // blue hood
                { 19, new HatMetadata(Content.HatMaskType.HideHair) }, // green hood
                { 20, new HatMetadata(Content.HatMaskType.HideHair) }, // red hood
                { 21, new HatMetadata(Content.HatMaskType.HideHair) }, // chainmail hat

                { 25, new HatMetadata(Content.HatMaskType.HideHair) }, // horned hat
                { 26, new HatMetadata(Content.HatMaskType.HideHair) }, // merchant hat
                { 28, new HatMetadata(Content.HatMaskType.HideHair) }, // helmy
                { 30, new HatMetadata(Content.HatMaskType.HideHair) }, // eloff helmet
                { 31, new HatMetadata(Content.HatMaskType.HideHair) }, // air hat

                { 32, new HatMetadata(Content.HatMaskType.FaceMask) }, // frog head
                { 33, new HatMetadata(Content.HatMaskType.FaceMask) }, // pilotte

                { 34, new HatMetadata(Content.HatMaskType.HideHair) }, // beruta
                { 35, new HatMetadata(Content.HatMaskType.HideHair) }, // pirate hat
                { 36, new HatMetadata(Content.HatMaskType.HideHair) }, // lotus helmet
                { 37, new HatMetadata(Content.HatMaskType.HideHair) }, // kitty hat
                { 38, new HatMetadata(Content.HatMaskType.HideHair) }, // hula hula hat

                { 40, new HatMetadata(Content.HatMaskType.HideHair) }, // gob helm
                { 41, new HatMetadata(Content.HatMaskType.HideHair) }, // horned gob helm
                { 44, new HatMetadata(Content.HatMaskType.HideHair) }, // helmet of darkness
                { 46, new HatMetadata(Content.HatMaskType.HideHair) }, // flad hat
                { 47, new HatMetadata(Content.HatMaskType.HideHair) }, // cook hat

                { 48, new HatMetadata(Content.HatMaskType.FaceMask) }, // glasses
                { 50, new HatMetadata(Content.HatMaskType.FaceMask) }, // medic cap
            };
        }
    }
}
