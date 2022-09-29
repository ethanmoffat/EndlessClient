using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class OtherCharacterEntityRenderer : BaseMapEntityRenderer
    {
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly ICharacterStateCache _characterStateCache;

        public OtherCharacterEntityRenderer(ICharacterProvider characterProvider,
                                            ICharacterRendererProvider characterRendererProvider,
                                            ICharacterStateCache characterStateCache,
                                            IGridDrawCoordinateCalculator gridDrawCoordinateCalculator)
            : base(characterProvider, gridDrawCoordinateCalculator)
        {
            _characterRendererProvider = characterRendererProvider;
            _characterStateCache = characterStateCache;
        }

        public override MapRenderLayer RenderLayer => MapRenderLayer.OtherCharacters;

        protected override int RenderDistance => 16;

        protected override bool ElementExistsAt(int row, int col)
        {
            return _characterStateCache.OtherCharacters.Values
                .Select(x => x.RenderProperties)
                .Any(c => c.MapY == row && c.MapX == col);
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            var idsToRender = _characterStateCache.OtherCharacters.Keys.Where(k => IsAtPosition(k, row, col));

            foreach (var id in idsToRender)
            {
                if (!_characterRendererProvider.CharacterRenderers.ContainsKey(id) ||
                    _characterRendererProvider.CharacterRenderers[id] == null)
                    return;

                var renderer = _characterRendererProvider.CharacterRenderers[id];
                renderer.DrawToSpriteBatch(spriteBatch);
            }
        }
        private bool IsAtPosition(int characterId, int row, int col)
        {
            var rp = _characterStateCache.OtherCharacters[characterId].RenderProperties;
            return row == rp.MapY && col == rp.MapX;
        }
    }
}