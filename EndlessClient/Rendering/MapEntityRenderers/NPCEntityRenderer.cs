// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.Rendering.Map;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class NPCEntityRenderer : BaseMapEntityRenderer
    {
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly INPCRendererProvider _npcRendererProvider;

        public NPCEntityRenderer(ICharacterProvider characterProvider,
                                 ICurrentMapStateProvider currentMapStateProvider,
                                 IRenderOffsetCalculator renderOffsetCalculator,
                                 INPCRendererProvider npcRendererProvider)
            : base(characterProvider, renderOffsetCalculator)
        {
            _currentMapStateProvider = currentMapStateProvider;
            _npcRendererProvider = npcRendererProvider;
        }

        public override MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.Npc; }
        }

        protected override int RenderDistance
        {
            get { return 16; }
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return _currentMapStateProvider.NPCs.Any(n => n.X == col && n.Y == row);
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            var indicesToRender = _currentMapStateProvider.NPCs
                .Where(npc => npc.X == col && npc.Y == row)
                .Select(npc => npc.Index);

            foreach (var index in indicesToRender)
            {
                if (!_npcRendererProvider.NPCRenderers.ContainsKey(index) ||
                    _npcRendererProvider.NPCRenderers[index] == null)
                    throw new InvalidOperationException(
                        $"Character renderer for ID {index} is null or missing! Did you call MapRenderer.Update() before calling MapRenderer.Draw()?");

                var renderer = _npcRendererProvider.NPCRenderers[index];
                renderer.DrawToSpriteBatch(spriteBatch);
            }
        }
    }
}
