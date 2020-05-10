using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class PassiveSpellsPanel : XNAPanel, IHudPanel
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        public PassiveSpellsPanel(INativeGraphicsManager nativeGraphicsManager)
        {
            _nativeGraphicsManager = nativeGraphicsManager;

            BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 62);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);
        }
    }
}