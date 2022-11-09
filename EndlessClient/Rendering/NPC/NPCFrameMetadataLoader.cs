using AutomaticTypeMapper;
using EOLib.Graphics;
using Newtonsoft.Json;
using Optional;
using PELoaderLib;
using System.Collections.Generic;
using System.Text;

namespace EndlessClient.Rendering.NPC
{
    [AutoMappedType(IsSingleton = true)]
    public class NPCMetadataLoader : INPCMetadataLoader
    {
        private readonly IPEFileCollection _peFileCollection;
        private readonly Dictionary<int, NPCFrameMetadata> _cache;

        public NPCMetadataLoader(IPEFileCollection peFileCollection)
        {
            _peFileCollection = peFileCollection;
            _cache = new Dictionary<int, NPCFrameMetadata>();
        }

        public Option<NPCFrameMetadata> GetMetadata(int graphic)
        {
            if (_cache.ContainsKey(graphic))
                return Option.Some(_cache[graphic]);

            try
            {
                var rawMetadata = _peFileCollection[GFXTypes.NPC].GetResourceByID(ResourceType.RCData, graphic);
                var metadataString = Encoding.Unicode.GetString(rawMetadata);
                _cache.Add(graphic, JsonConvert.DeserializeObject<NPCFrameMetadata>(metadataString));

                return _cache[graphic].SomeNotNull();
            }
            catch
            {
                return Option.None<NPCFrameMetadata>();
            }
        }
    }

    public interface INPCMetadataLoader
    {
        Option<NPCFrameMetadata> GetMetadata(int graphic);
    }
}
