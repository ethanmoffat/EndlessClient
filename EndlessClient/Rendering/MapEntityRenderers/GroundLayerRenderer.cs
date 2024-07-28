using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers;

public class GroundLayerRenderer : BaseMapEntityRenderer
{
    protected const int ANIMATED_TILE_MIN_WIDTH = 128;

    protected readonly INativeGraphicsManager _nativeGraphicsManager;
    private readonly ICurrentMapProvider _currentMapProvider;

    public override MapRenderLayer RenderLayer => MapRenderLayer.Ground;

    protected override int RenderDistance => 10;

    public GroundLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                               ICurrentMapProvider currentMapProvider,
                               ICharacterProvider characterProvider,
                               IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                               IClientWindowSizeProvider clientWindowSizeProvider)
        : base(characterProvider, gridDrawCoordinateCalculator, clientWindowSizeProvider)
    {
        _nativeGraphicsManager = nativeGraphicsManager;
        _currentMapProvider = currentMapProvider;
    }

    protected override Vector2 GetDrawCoordinatesFromGridUnits(int gridX, int gridY)
    {
        // the height is used to offset the 0 point of the grid, which is 32 units per tile in the height of the map
        var height = CurrentMap.Properties.Height;
        var offset = new Vector2(32 * height, 0);

        var basePosition = _gridDrawCoordinateCalculator.CalculateRawRenderCoordinatesFromGridUnits(gridX, gridY);
        return basePosition + offset;
    }

    public override bool CanRender(int row, int col) => ElementExistsAt(row, col);

    protected override bool ElementExistsAt(int row, int col)
    {
        return (CurrentMap.Properties.FillTile > 0 && CurrentMap.GFX[MapLayer.GroundTile][row, col] < 0) ||
            CurrentMap.GFX[MapLayer.GroundTile][row, col] > 0;
    }

    public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
    {
        base.RenderElementAt(spriteBatch, row, col, alpha);

        var pos = GetDrawCoordinatesFromGridUnits(col, row);

        int tileGFX;
        Texture2D tileTexture;
        if ((tileGFX = CurrentMap.Properties.FillTile) > 0 && CurrentMap.GFX[MapLayer.GroundTile][row, col] < 0)
        {
            tileTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapTiles, tileGFX, true);
        }
        else if ((tileGFX = CurrentMap.GFX[MapLayer.GroundTile][row, col]) > 0)
        {
            tileTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapTiles, tileGFX, true);
        }
        else
        {
            return;
        }

        var src = tileTexture.Width >= ANIMATED_TILE_MIN_WIDTH
            ? new Rectangle?(new Rectangle((tileTexture.Width / 4) * _frameIndex, 0, tileTexture.Width / 4, tileTexture.Height))
            : null;
        spriteBatch.Draw(tileTexture, pos + additionalOffset, src, Color.FromNonPremultiplied(255, 255, 255, alpha));
    }

    protected IMapFile CurrentMap => _currentMapProvider.CurrentMap;
}

public class AnimatedGroundLayerRenderer : GroundLayerRenderer
{
    public AnimatedGroundLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                       ICurrentMapProvider currentMapProvider,
                                       ICharacterProvider characterProvider,
                                       IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                       IClientWindowSizeProvider clientWindowSizeProvider)
        : base(nativeGraphicsManager, currentMapProvider, characterProvider, gridDrawCoordinateCalculator, clientWindowSizeProvider)
    {
    }

    protected override bool ElementExistsAt(int row, int col)
    {
        int tileId;
        var tileExists = ((tileId = CurrentMap.Properties.FillTile) > 0 && CurrentMap.GFX[MapLayer.GroundTile][row, col] < 0) ||
            (tileId = CurrentMap.GFX[MapLayer.GroundTile][row, col]) > 0;

        return tileExists && _nativeGraphicsManager.TextureFromResource(GFXTypes.MapTiles, tileId, true).Width >= ANIMATED_TILE_MIN_WIDTH;
    }

    protected override Vector2 GetDrawCoordinatesFromGridUnits(int gridX, int gridY)
    {
        return _gridDrawCoordinateCalculator.CalculateBaseLayerDrawCoordinatesFromGridUnits(gridX, gridY);
    }
}