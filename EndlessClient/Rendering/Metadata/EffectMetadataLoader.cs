using AutomaticTypeMapper;
using EndlessClient.Rendering.Metadata.Models;
using EOLib.Graphics;
using Newtonsoft.Json;
using Optional;
using PELoaderLib;
using System.Collections.Generic;
using System.Text;

namespace EndlessClient.Rendering.Metadata
{
    [AutoMappedType(IsSingleton = true)]
    public class EffectMetadataLoader : IEffectMetadataLoader
    {
        private readonly IPEFileCollection _peFileCollection;
        private readonly Dictionary<int, EffectMetadata> _cache;

        public EffectMetadataLoader(IPEFileCollection peFileCollection)
        {
            _peFileCollection = peFileCollection;
            _cache = new Dictionary<int, EffectMetadata>();
        }

        public Option<EffectMetadata> GetEffectMetadata(int graphic)
        {
            if (_cache.ContainsKey(graphic))
                return _cache[graphic].SomeNotNull();

            try
            {
                var rawMetadata = _peFileCollection[GFXTypes.Spells].GetResourceByID(ResourceType.RCData, graphic);
                var metadataString = Encoding.Unicode.GetString(rawMetadata);
                _cache.Add(graphic, JsonConvert.DeserializeObject<EffectMetadata>(metadataString));

                return _cache[graphic].SomeNotNull();
            }
            catch
            {
                return Option.None<EffectMetadata>();
            }
        }
    }

    public interface IEffectMetadataLoader
    {
        Option<EffectMetadata> GetEffectMetadata(int graphic);
    }
}
