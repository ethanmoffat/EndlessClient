using AutomaticTypeMapper;
using EOLib.Config;
using PELoaderLib;
using SixLabors.ImageSharp;

namespace EOLib.Graphics
{
    [MappedType(BaseType = typeof(INativeGraphicsLoader), IsSingleton = true)]
    public class NativeGraphicsLoader : INativeGraphicsLoader
    {
        private const int CULTURE_EN_US = 1033;

        private readonly IPEFileCollection _modules;
        private readonly IConfigurationProvider _configurationProvider;

        public NativeGraphicsLoader(IPEFileCollection modules,
                                    IConfigurationProvider configurationProvider)
        {
            _modules = modules;
            _configurationProvider = configurationProvider;
        }

        public IImage LoadGFX(GFXTypes file, int resourceValue)
        {
            var version = _configurationProvider.MainCloneCompat ? BitmapVersion.BitmapV3InfoHeader : BitmapVersion.BitmapInfoHeader;
            var culture = _configurationProvider.MainCloneCompat ? CULTURE_EN_US : 0;
            var fileBytes = _modules[file].GetEmbeddedBitmapResourceByID(resourceValue + 100, version, culture);

            if (fileBytes.Length == 0)
                throw new GFXLoadException(resourceValue, file);

            return Image.Load(fileBytes);
        }
    }
}
