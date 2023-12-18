using AutomaticTypeMapper;
using EndlessClient.Rendering.Metadata.Models;
using EOLib.Graphics;
using Newtonsoft.Json;
using Optional;
using PELoaderLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace EndlessClient.Rendering.Metadata
{
    [AutoMappedType(IsSingleton = true)]
    public class GFXMetadataLoader : IGFXMetadataLoader
    {
        private readonly IPEFileCollection _peFileCollection;
        private readonly Dictionary<GFXTypes, Dictionary<int, IGFXMetadata>> _cache;

        private static readonly Dictionary<Type, GFXTypes> _mapper;

        static GFXMetadataLoader()
        {
            _mapper = new Dictionary<Type, GFXTypes>
            {
                { typeof(EffectMetadata), GFXTypes.Spells },
                { typeof(NPCMetadata), GFXTypes.NPC },
                { typeof(ShieldMetadata), GFXTypes.MaleBack },
                { typeof(HatMetadata), GFXTypes.MaleHat },
                { typeof(WeaponMetadata), GFXTypes.MaleWeapons }
            };
        }

        public GFXMetadataLoader(IPEFileCollection peFileCollection)
        {
            _peFileCollection = peFileCollection;
            _cache = new Dictionary<GFXTypes, Dictionary<int, IGFXMetadata>>();
        }

        public Option<TMetadata> GetMetadata<TMetadata>(int graphic)
            where TMetadata : class, IGFXMetadata
        {
            if (!_mapper.TryGetValue(typeof(TMetadata), out var gfxType))
                return Option.None<TMetadata>();

            if (!_cache.ContainsKey(gfxType))
                _cache.Add(gfxType, new Dictionary<int, IGFXMetadata>());
            else if (_cache[gfxType].ContainsKey(graphic))
                return Option.Some(_cache[gfxType][graphic] as TMetadata).NotNull();

            try
            {
                var rawMetadata = _peFileCollection[gfxType].GetResourceByID(ResourceType.RCData, graphic);
                var metadataString = Encoding.Unicode.GetString(rawMetadata);
                _cache[gfxType].Add(graphic, JsonConvert.DeserializeObject<TMetadata>(metadataString));

                return (_cache[gfxType][graphic] as TMetadata).SomeNotNull();
            }
            catch
            {
                return Option.None<TMetadata>();
            }
        }
    }

    public interface IGFXMetadataLoader
    {
        Option<TMetadata> GetMetadata<TMetadata>(int graphic)
            where TMetadata : class, IGFXMetadata;
    }
}
