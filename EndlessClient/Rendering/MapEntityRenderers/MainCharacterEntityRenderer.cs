using System;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class MainCharacterEntityRenderer : BaseMapEntityRenderer
    {
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IChatBubbleProvider _chatBubbleProvider;
        private readonly bool _transparent;

        public MainCharacterEntityRenderer(ICharacterProvider characterProvider,
                                           ICharacterRendererProvider characterRendererProvider,
                                           IChatBubbleProvider chatBubbleProvider,
                                           IRenderOffsetCalculator renderOffsetCalculator,
                                           bool transparent)
            : base(characterProvider, renderOffsetCalculator)
        {
            _characterRendererProvider = characterRendererProvider;
            _chatBubbleProvider = chatBubbleProvider;
            _transparent = transparent;
        }

        public override MapRenderLayer RenderLayer =>
            _transparent ? MapRenderLayer.MainCharacterTransparent : MapRenderLayer.MainCharacter;

        protected override int RenderDistance => 1;

        protected override bool ElementExistsAt(int row, int col)
        {
            return row == _characterProvider.MainCharacter.RenderProperties.MapY &&
                   col == _characterProvider.MainCharacter.RenderProperties.MapX;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            if (_characterRendererProvider.MainCharacterRenderer == null)
                return;

            _characterRendererProvider.MainCharacterRenderer.Transparent = _transparent;
            _characterRendererProvider.MainCharacterRenderer.DrawToSpriteBatch(spriteBatch);

            if (_chatBubbleProvider.MainCharacterChatBubble.HasValue)
                _chatBubbleProvider.MainCharacterChatBubble.Value.DrawToSpriteBatch(spriteBatch);
        }
    }
}
