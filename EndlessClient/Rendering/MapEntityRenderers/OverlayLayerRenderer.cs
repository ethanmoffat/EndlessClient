// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class OverlayLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        public override MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.Overlay; }
        }

        protected override int RenderDistance
        {
            get { return 10; }
        }

        public OverlayLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                    IMapFileProvider mapFileProvider,
                                    ICharacterProvider characterProvider,
                                    ICharacterRenderOffsetCalculator characterRenderOffsetCalculator)
            : base(mapFileProvider, characterProvider, characterRenderOffsetCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            int gfxNum;
            if ((gfxNum = MapFile.GFX[MapLayer.OverlayObjects][row, col]) <= 0)
                return;

            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapOverlay, gfxNum, true);
            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            pos = new Vector2(pos.X + 16, pos.Y - 11);
            spriteBatch.Draw(gfx, pos, Color.FromNonPremultiplied(255, 255, 255, alpha));
        }
    }
}
