// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class OnTopLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;

        public override MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.OnTop; }
        }

        protected override int RenderDistance { get { return 12; } }

        public OnTopLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                  ICurrentMapProvider currentMapProvider,
                                  ICharacterProvider characterProvider,
                                  IRenderOffsetCalculator renderOffsetCalculator)
            : base(characterProvider, renderOffsetCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return CurrentMap.GFX[MapLayer.OverlayTile][row, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            int gfxNum = CurrentMap.GFX[MapLayer.OverlayTile][row, col];
            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapTiles, gfxNum, true);

            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            pos = new Vector2(pos.X - 2, pos.Y - 31);

            spriteBatch.Draw(gfx, pos, Color.FromNonPremultiplied(255, 255, 255, alpha));
        }

        private IMapFile CurrentMap { get { return _currentMapProvider.CurrentMap; } }
    }
}
