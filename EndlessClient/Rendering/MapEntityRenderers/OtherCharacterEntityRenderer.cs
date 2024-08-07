using System.Linq;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class OtherCharacterEntityRenderer : BaseMapEntityRenderer
    {
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        public OtherCharacterEntityRenderer(ICharacterProvider characterProvider,
                                            ICharacterRendererProvider characterRendererProvider,
                                            ICurrentMapStateProvider currentMapStateProvider,
                                            IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                            IClientWindowSizeProvider clientWindowSizeProvider)
            : base(characterProvider, gridDrawCoordinateCalculator, clientWindowSizeProvider)
        {
            _characterRendererProvider = characterRendererProvider;
            _currentMapStateProvider = currentMapStateProvider;
        }

        public override MapRenderLayer RenderLayer => MapRenderLayer.OtherCharacters;

        protected override int RenderDistance => 16;

        protected override bool ElementExistsAt(int row, int col)
        {
            return _currentMapStateProvider.Characters.ContainsKey(new MapCoordinate(col, row));
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            var toRender = _currentMapStateProvider.Characters[new MapCoordinate(col, row)];

            foreach (var id in toRender.Select(x => x.ID))
            {
                if (!_characterRendererProvider.CharacterRenderers.ContainsKey(id) ||
                    _characterRendererProvider.CharacterRenderers[id] == null)
                    return;

                _characterRendererProvider.CharacterRenderers[id].DrawToSpriteBatch(spriteBatch);
            }
        }
    }
}
