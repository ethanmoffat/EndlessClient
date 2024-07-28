using EndlessClient.Rendering;
using EOLib.Graphics;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.Panels;

public class PassiveSpellsPanel : DraggableHudPanel
{
    private readonly INativeGraphicsManager _nativeGraphicsManager;

    public PassiveSpellsPanel(INativeGraphicsManager nativeGraphicsManager,
                              IClientWindowSizeProvider clientWindowSizeProvider)
        : base(clientWindowSizeProvider.Resizable)
    {
        _nativeGraphicsManager = nativeGraphicsManager;

        BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 62);
        DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);
    }
}