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
    public class RoofLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;

        public override MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.Roof; }
        }

        protected override int RenderDistance
        {
            get { return 12; }
        }

        public RoofLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                 ICurrentMapProvider currentMapProvider,
                                 ICharacterProvider characterProvider,
                                 ICharacterRenderOffsetCalculator characterRenderOffsetCalculator)
            : base(characterProvider, characterRenderOffsetCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return CurrentMap.GFX[MapLayer.Roof][row, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            int gfxNum = CurrentMap.GFX[MapLayer.Roof][row, col];
            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapOverlay, gfxNum, true);

            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            pos = new Vector2(pos.X - gfx.Width / 2f + 30, pos.Y - gfx.Height + 28);

            spriteBatch.Draw(gfx, pos, Color.FromNonPremultiplied(255, 255, 255, alpha));
        }

        private IMapFile CurrentMap { get { return _currentMapProvider.CurrentMap; } }
    }
}
