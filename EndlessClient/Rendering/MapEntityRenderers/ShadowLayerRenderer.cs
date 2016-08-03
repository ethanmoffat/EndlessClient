// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Map;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class ShadowLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IConfigurationProvider _configurationProvider;

        public override MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.Shadows; }
        }

        protected override int RenderDistance
        {
            get { return 10; }
        }

        public ShadowLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                   ICurrentMapProvider currentMapProvider,
                                   ICharacterProvider characterProvider,
                                   ICharacterRenderOffsetCalculator characterRenderOffsetCalculator,
                                   IConfigurationProvider configurationProvider)
            : base(characterProvider, characterRenderOffsetCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
            _configurationProvider = configurationProvider;
        }

        public override bool CanRender(int row, int col)
        {
            return _configurationProvider.ShowShadows && base.CanRender(row, col);
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            int gfxNum;
            if ((gfxNum = CurrentMap.GFX[MapLayer.Shadow][row, col]) <= 0)
                return;

            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.Shadows, gfxNum, true);
            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            pos = new Vector2(pos.X - 24, pos.Y - 12);
            spriteBatch.Draw(gfx, pos, Color.FromNonPremultiplied(255, 255, 255, 60));
        }

        private IReadOnlyMapFile CurrentMap { get { return _currentMapProvider.CurrentMap; } }
    }
}
