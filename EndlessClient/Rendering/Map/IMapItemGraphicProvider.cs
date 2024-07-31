using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Map
{
    public interface IMapItemGraphicProvider
    {
        Texture2D GetItemGraphic(int id, int amount);
    }
}