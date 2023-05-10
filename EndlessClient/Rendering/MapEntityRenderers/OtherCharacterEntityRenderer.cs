using EndlessClient.Rendering.Character;
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
                                            IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                            IClientWindowSizeProvider clientWindowSizeProvider)
            : base(characterProvider, gridDrawCoordinateCalculator, clientWindowSizeProvider)
        {
            _characterRendererProvider = characterRendererProvider;
            _characterStateCache = characterStateCache;
        }

        public override MapRenderLayer RenderLayer => MapRenderLayer.OtherCharacters;

        protected override int RenderDistance => 16;

        protected override bool ElementExistsAt(int row, int col)
        {
            return _characterStateCache.OtherCharacters.Values
                .Any(IsCharAt);

            bool IsCharAt(EOLib.Domain.Character.Character c) => OtherCharacterEntityRenderer.IsCharAt(c, row, col);
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            var toRender = _characterStateCache.OtherCharacters.Values.Where(IsCharAt);

            foreach (var rend in toRender)
            {
                var id = rend.ID;

                if (!_characterRendererProvider.CharacterRenderers.ContainsKey(id) ||
                    _characterRendererProvider.CharacterRenderers[id] == null)
                    return;

                var renderer = _characterRendererProvider.CharacterRenderers[id];
                renderer.DrawToSpriteBatch(spriteBatch);
            }

            bool IsCharAt(EOLib.Domain.Character.Character c) => OtherCharacterEntityRenderer.IsCharAt(c, row, col);
        }
        private static bool IsCharAt(EOLib.Domain.Character.Character c, int row, int col)
        {
            return row == c.Y && col == c.X;
        }
    }
}