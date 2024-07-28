using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers;

public class Overlay2LayerRenderer : BaseMapEntityRenderer
{
    private readonly INativeGraphicsManager _nativeGraphicsManager;
    private readonly ICurrentMapProvider _currentMapProvider;

    public override MapRenderLayer RenderLayer => MapRenderLayer.Overlay2;

    protected override int RenderDistance => 12;

    public Overlay2LayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                 ICurrentMapProvider currentMapProvider,
                                 ICharacterProvider characterProvider,
                                 IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                 IClientWindowSizeProvider clientWindowSizeProvider)
        : base(characterProvider, gridDrawCoordinateCalculator, clientWindowSizeProvider)
    {
        _nativeGraphicsManager = nativeGraphicsManager;
        _currentMapProvider = currentMapProvider;
    }

    protected override bool ElementExistsAt(int row, int col)
    {
        return CurrentMap.GFX[MapLayer.Overlay2][row, col] > 0;
    }

    public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
    {
        int gfxNum = CurrentMap.GFX[MapLayer.Overlay2][row, col];
        var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapOverlay, gfxNum, true);

        var pos = GetDrawCoordinatesFromGridUnits(col, row);
        pos -= new Vector2((float)System.Math.Floor(gfx.Width / 2f), gfx.Height - 32);

        spriteBatch.Draw(gfx, pos + additionalOffset, Color.FromNonPremultiplied(255, 255, 255, alpha));
    }

    private IMapFile CurrentMap => _currentMapProvider.CurrentMap;
}