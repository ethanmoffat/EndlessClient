using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class MainCharacterEntityRenderer : BaseMapEntityRenderer
    {
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly bool _transparent;

        public MainCharacterEntityRenderer(ICharacterProvider characterProvider,
                                           ICharacterRendererProvider characterRendererProvider,
                                           IRenderOffsetCalculator renderOffsetCalculator,
                                           bool transparent)
            : base(characterProvider, renderOffsetCalculator)
        {
            _characterRendererProvider = characterRendererProvider;
            _transparent = transparent;
        }

        public override MapRenderLayer RenderLayer =>
            _transparent ? MapRenderLayer.MainCharacterTransparent : MapRenderLayer.MainCharacter;

        protected override int RenderDistance => 1;

        protected override bool ElementExistsAt(int row, int col)
        {
            var rp = _characterProvider.MainCharacter.RenderProperties;
            if (!rp.IsActing(CharacterActionState.Walking))
            {
                return row == rp.MapY && col == rp.MapX;
            }
            else
            {
                return row == rp.GetDestinationY() && col == rp.GetDestinationX();
            }
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            if (_characterRendererProvider.MainCharacterRenderer == null)
                return;

            _characterRendererProvider.MainCharacterRenderer.Transparent = _transparent;
            _characterRendererProvider.MainCharacterRenderer.DrawToSpriteBatch(spriteBatch);
        }
    }
}
