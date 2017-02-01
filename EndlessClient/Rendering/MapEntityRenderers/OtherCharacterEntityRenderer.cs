// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class OtherCharacterEntityRenderer : BaseMapEntityRenderer
    {
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly ICharacterStateCache _characterStateCache;

        public OtherCharacterEntityRenderer(ICharacterProvider characterProvider,
                                            ICharacterRendererProvider characterRendererProvider,
                                            ICharacterStateCache characterStateCache,
                                            IRenderOffsetCalculator renderOffsetCalculator)
            : base(characterProvider, renderOffsetCalculator)
        {
            _characterRendererProvider = characterRendererProvider;
            _characterStateCache = characterStateCache;
        }

        public override MapRenderLayer RenderLayer => MapRenderLayer.OtherCharacters;

        protected override int RenderDistance => 16;

        protected override bool ElementExistsAt(int row, int col)
        {
            return _characterStateCache.CharacterRenderProperties.Values.Any(c => c.MapY == row && c.MapX == col);
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            var idsToRender = _characterStateCache.CharacterRenderProperties.Keys.Where(
                key => _characterStateCache.CharacterRenderProperties[key].MapX == col &&
                       _characterStateCache.CharacterRenderProperties[key].MapY == row);

            foreach (var id in idsToRender)
            {
                if (!_characterRendererProvider.CharacterRenderers.ContainsKey(id) ||
                    _characterRendererProvider.CharacterRenderers[id] == null)
                    throw new InvalidOperationException(
                        $"Character renderer for ID {id} is null or missing! Did you call MapRenderer.Update() before calling MapRenderer.Draw()?");

                var renderer = _characterRendererProvider.CharacterRenderers[id];
                renderer.DrawToSpriteBatch(spriteBatch);
            }
        }
    }
}