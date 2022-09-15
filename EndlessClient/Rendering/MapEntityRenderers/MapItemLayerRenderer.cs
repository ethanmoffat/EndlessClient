using System;
using System.Linq;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class MapItemLayerRenderer : BaseMapEntityRenderer
    {
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IMapItemGraphicProvider _mapItemGraphicProvider;

        public override MapRenderLayer RenderLayer => MapRenderLayer.Item;

        protected override int RenderDistance => 16;

        public MapItemLayerRenderer(ICharacterProvider characterProvider,
                                    IRenderOffsetCalculator renderOffsetCalculator,
                                    ICurrentMapStateProvider currentMapStateProvider,
                                    IMapItemGraphicProvider mapItemGraphicProvider)
            : base(characterProvider, renderOffsetCalculator)
        {
            _currentMapStateProvider = currentMapStateProvider;
            _mapItemGraphicProvider = mapItemGraphicProvider;
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return _currentMapStateProvider.MapItems.Any(item => item.X == col && item.Y == row);
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            var items = _currentMapStateProvider
                .MapItems
                .Where(item => item.X == col && item.Y == row)
                .OrderBy(item => item.UniqueID);

            foreach (var item in items)
            {
                //note: col is offset by 1. I'm not sure why this is needed. Maybe I did something wrong when translating the packets...
                var itemPos = GetDrawCoordinatesFromGridUnits(col, row);
                var itemTexture = _mapItemGraphicProvider.GetItemGraphic(item.ItemID, item.Amount);

                spriteBatch.Draw(itemTexture,
                                 new Vector2(itemPos.X - (int) Math.Round(itemTexture.Width/2.0),
                                             itemPos.Y - (int) Math.Round(itemTexture.Height/2.0)) + additionalOffset,
                                 Color.FromNonPremultiplied(255, 255, 255, alpha));
            }
        }
    }
}
