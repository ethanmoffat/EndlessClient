using AutomaticTypeMapper;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Map
{
    [AutoMappedType]
    public class MapItemGraphicProvider : IMapItemGraphicProvider
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEIFFileProvider _eifFileProvider;

        public MapItemGraphicProvider(INativeGraphicsManager nativeGraphicsManager, IEIFFileProvider eifFileProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _eifFileProvider = eifFileProvider;
        }

        public Texture2D GetItemGraphic(int id, int amount)
        {
            var item = _eifFileProvider.EIFFile[id];

            if (item.Type == ItemType.Money)
            {
                var gfx = amount >= 100000 ? 4 : (
                    amount >= 10000 ? 3 : (
                        amount >= 100 ? 2 : (
                            amount >= 2 ? 1 : 0)));
                return _nativeGraphicsManager.TextureFromResource(GFXTypes.Items, 269 + 2*gfx, true);
            }

            return _nativeGraphicsManager.TextureFromResource(GFXTypes.Items, 2*item.Graphic - 1, true);
        }
    }
}