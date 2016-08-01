// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class MapItemLayerRenderer : BaseMapEntityRenderer
    {
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IMapItemGraphicProvider _mapItemGraphicProvider;

        public override MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.Item; }
        }

        protected override int RenderDistance
        {
            get { return 16; }
        }

        public MapItemLayerRenderer(IMapFileProvider mapFileProvider,
                                    ICharacterProvider characterProvider,
                                    ICharacterRenderOffsetCalculator characterRenderOffsetCalculator,
                                    ICurrentMapStateProvider currentMapStateProvider,
                                    IMapItemGraphicProvider mapItemGraphicProvider)
            : base(mapFileProvider, characterProvider, characterRenderOffsetCalculator)
        {
            _currentMapStateProvider = currentMapStateProvider;
            _mapItemGraphicProvider = mapItemGraphicProvider;
        }

        public override bool ElementTypeIsInRange(int row, int col)
        {
            if (!base.ElementTypeIsInRange(row, col))
                return false;

            return _currentMapStateProvider.MapItems.Any(item => item.X == col && item.Y == row);
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            var items = _currentMapStateProvider.MapItems.Where(item => item.X == col && item.Y == row);

            foreach (var item in items)
            {
                //note: col is offset by 1. I'm not sure why this is needed. Maybe I did something wrong when translating the packets...
                var itemPos = GetDrawCoordinatesFromGridUnits(col + 1, row);
                var itemTexture = _mapItemGraphicProvider.GetItemGraphic(item.ItemID, item.Amount);

                spriteBatch.Draw(itemTexture,
                                 new Vector2(itemPos.X - (int) Math.Round(itemTexture.Width/2.0),
                                             itemPos.Y - (int) Math.Round(itemTexture.Height/2.0)),
                                 Color.FromNonPremultiplied(255, 255, 255, alpha));
            }
        }
    }
}
