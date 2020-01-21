using AutomaticTypeMapper;
using EOLib.Config;
using System;
using System.Collections.Generic;

namespace EndlessClient.Content
{
    public interface IHatConfigurationRepository
    {
        IDictionary<int, HatMaskType> HatMasks { get; set; }
    }

    public interface IHatConfigurationProvider
    {
        IReadOnlyDictionary<int, HatMaskType> HatMasks { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class HatConfigurationRepository : IHatConfigurationRepository, IHatConfigurationProvider
    {
        public IDictionary<int, HatMaskType> HatMasks { get; set; }

        IReadOnlyDictionary<int, HatMaskType> IHatConfigurationProvider.HatMasks => (IReadOnlyDictionary<int, HatMaskType>)HatMasks;

        public HatConfigurationRepository()
        {
            HatMasks = new Dictionary<int, HatMaskType>();

            var reader = new IniReader("ContentPipeline/HairClipTypes.ini");
            if (!reader.Load())
                throw new MalformedConfigException("Unable to load the HairClipTypes configuration file");

            if (!reader.Sections.ContainsKey("Hats"))
                throw new MalformedConfigException("[Hats] section not found in HairClipTypes configuration file");

            foreach (var pair in reader.Sections["Hats"])
            {
                if (!int.TryParse(pair.Key, out var id))
                    throw new MalformedConfigException($"Error parsing {pair.Key} in HairClipTypes configuration file");
                if (!Enum.TryParse(pair.Value, ignoreCase: true, result: out HatMaskType maskType))
                    throw new MalformedConfigException($"Error parsing {id}={pair.Value} in HairClipTypes configuration file");

                HatMasks.Add(id, maskType);
            }
        }
    }

    public enum HatMaskType
    {
        Standard,
        FaceMask,
        HideHair
    }
}
