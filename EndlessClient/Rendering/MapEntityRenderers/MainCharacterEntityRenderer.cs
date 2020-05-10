using System;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class MainCharacterEntityRenderer : BaseMapEntityRenderer
    {
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IChatBubbleProvider _chatBubbleProvider;
        private readonly BlendState _playerBlend;

        public MainCharacterEntityRenderer(ICharacterProvider characterProvider,
                                           ICharacterRendererProvider characterRendererProvider,
                                           IChatBubbleProvider chatBubbleProvider,
                                           IRenderOffsetCalculator renderOffsetCalculator)
            : base(characterProvider, renderOffsetCalculator)
        {
            _characterRendererProvider = characterRendererProvider;
            _chatBubbleProvider = chatBubbleProvider;

            _playerBlend = new BlendState
            {
                BlendFactor = new Color(255, 255, 255, 64),

                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                AlphaBlendFunction = BlendFunction.Add,

                ColorSourceBlend = Blend.BlendFactor,
                ColorDestinationBlend = Blend.One
            };
        }

        public override MapRenderLayer RenderLayer => MapRenderLayer.MainCharacter;

        protected override int RenderDistance => 1;

        protected override bool ElementExistsAt(int row, int col)
        {
            return row == _characterProvider.MainCharacter.RenderProperties.MapY &&
                   col == _characterProvider.MainCharacter.RenderProperties.MapX;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            if(_characterRendererProvider.MainCharacterRenderer == null)
                throw new InvalidOperationException("Main character renderer is null! Did you call MapRenderer.Update() before calling MapRenderer.Draw()?");

            spriteBatch.End();

            var blendState = _characterProvider.MainCharacter.RenderProperties.IsHidden
                ? BlendState.NonPremultiplied
                : _playerBlend;
            spriteBatch.Begin(SpriteSortMode.Deferred, blendState);

            _characterRendererProvider.MainCharacterRenderer.DrawToSpriteBatch(spriteBatch);
            if (_chatBubbleProvider.MainCharacterChatBubble.HasValue)
                _chatBubbleProvider.MainCharacterChatBubble.Value.DrawToSpriteBatch(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _playerBlend.Dispose();
            base.Dispose(disposing);
        }
    }
}
