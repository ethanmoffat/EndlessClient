using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public interface IHudPanel : IGameComponent
    {
        bool Visible { get; set; }

        Vector2 DrawPosition { get; set; }

        Rectangle DrawArea { get; set; }
    }
}
