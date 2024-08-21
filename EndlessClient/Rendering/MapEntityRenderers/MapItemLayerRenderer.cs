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
                                    IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                    IClientWindowSizeProvider clientWindowSizeProvider,
                                    ICurrentMapStateProvider currentMapStateProvider,
                                    IMapItemGraphicProvider mapItemGraphicProvider)
            : base(characterProvider, gridDrawCoordinateCalculator, clientWindowSizeProvider)
        {
            _currentMapStateProvider = currentMapStateProvider;
            _mapItemGraphicProvider = mapItemGraphicProvider;
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return _currentMapStateProvider.MapItems.ContainsKey(new MapCoordinate(col, row));
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            var items = _currentMapStateProvider.MapItems[new MapCoordinate(col, row)];

            foreach (var item in items.OrderBy(item => item.UniqueID))
            {
                var itemPos = GetDrawCoordinatesFromGridUnits(col, row);
                var itemTexture = _mapItemGraphicProvider.GetItemGraphic(item.ItemID, item.Amount);

                spriteBatch.Draw(itemTexture,
                                 new Vector2(itemPos.X - (int)Math.Round(itemTexture.Width / 2.0),
                                             itemPos.Y - (int)Math.Round(itemTexture.Height / 2.0)) + additionalOffset,
                                 Color.FromNonPremultiplied(255, 255, 255, alpha));
            }
        }
    }
}
