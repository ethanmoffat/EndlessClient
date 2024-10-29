using System.Collections.Generic;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class RoofLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;

        public override MapRenderLayer RenderLayer => MapRenderLayer.Roof;

        protected override int RenderDistance => 12;

        public RoofLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
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
            if (row == CurrentMap.Properties.Height)
            {
                return CurrentMap.GFX[MapLayer.Roof][row, col] > 0 ||
                    CurrentMap.GFX[MapLayer.Roof][row - 1, col] > 0;
            }

            return row - 1 >= 0 && CurrentMap.GFX[MapLayer.Roof][row - 1, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            var gfxCandidates = new List<int>();
            if (CurrentMap.GFX[MapLayer.Roof][row - 1, col] > 0)
                gfxCandidates.Add(CurrentMap.GFX[MapLayer.Roof][row - 1, col]);
            if (row == CurrentMap.Properties.Height)
            {
                if (CurrentMap.GFX[MapLayer.Roof][row, col] > 0)
                    gfxCandidates.Add(CurrentMap.GFX[MapLayer.Roof][row, col]);
            }

            //int gfxNum = CurrentMap.GFX[MapLayer.Roof][row-1, col];
            foreach (var gfxNum in gfxCandidates)
            {
                var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapWallTop, gfxNum, true);

                var pos = GetDrawCoordinatesFromGridUnits(col, row - 1);
                spriteBatch.Draw(gfx, pos + additionalOffset, Color.FromNonPremultiplied(255, 255, 255, alpha));
            }
        }

        private IMapFile CurrentMap => _currentMapProvider.CurrentMap;
    }
}
