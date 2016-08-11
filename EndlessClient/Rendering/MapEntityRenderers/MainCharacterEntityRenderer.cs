// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class MainCharacterEntityRenderer : BaseMapEntityRenderer
    {
        private readonly ICharacterRendererProvider _characterRendererProvider;

        public MainCharacterEntityRenderer(ICharacterProvider characterProvider,
                                           ICharacterRendererProvider characterRendererProvider,
                                           ICharacterRenderOffsetCalculator characterRenderOffsetCalculator)
            : base(characterProvider, characterRenderOffsetCalculator)
        {
            _characterRendererProvider = characterRendererProvider;
        }

        public override MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.MainCharacter; }
        }

        protected override int RenderDistance
        {
            get { return 1; }
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return row == _characterProvider.MainCharacter.RenderProperties.MapY &&
                   col == _characterProvider.MainCharacter.RenderProperties.MapX;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            if(_characterRendererProvider.MainCharacterRenderer == null)
                throw new InvalidOperationException("Active character renderer is null! Did you call MapRenderer.Update() before calling MapRenderer.Draw()?");

            spriteBatch.End();

            //todo: use different blend state if character is hidden
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            _characterRendererProvider.MainCharacterRenderer.DrawToSpriteBatch(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
        }
    }
}
