using EndlessClient.Rendering;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.HUD.Panels;

public class HelpPanel : DraggableHudPanel
{
    private readonly INativeGraphicsManager _nativeGraphicsManager;

    public HelpPanel(INativeGraphicsManager nativeGraphicsManager,
                     IClientWindowSizeProvider clientWindowSizeProvider)
        : base(clientWindowSizeProvider.Resizable)
    {
        _nativeGraphicsManager = nativeGraphicsManager;

        BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 63);
        DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);
    }
}