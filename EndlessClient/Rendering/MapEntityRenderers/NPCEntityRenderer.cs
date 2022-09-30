using EndlessClient.Rendering.Map;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.NPC;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class NPCEntityRenderer : BaseMapEntityRenderer
    {
        private readonly INPCRendererProvider _npcRendererProvider;

        public NPCEntityRenderer(ICharacterProvider characterProvider,
                                 IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                 IClientWindowSizeProvider clientWindowSizeProvider,
                                 INPCRendererProvider npcRendererProvider)
            : base(characterProvider, gridDrawCoordinateCalculator, clientWindowSizeProvider)
        {
            _npcRendererProvider = npcRendererProvider;
        }

        public override MapRenderLayer RenderLayer => MapRenderLayer.Npc;

        protected override int RenderDistance => 16;

        protected override bool ElementExistsAt(int row, int col)
        {
            return _npcRendererProvider.NPCRenderers.Values
                .Count(n => IsNpcAt(n.NPC, row, col)) > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            var indicesToRender = _npcRendererProvider.NPCRenderers.Values
                .Where(n => IsNpcAt(n.NPC, row, col))
                .Select(n => n.NPC.Index);

            foreach (var index in indicesToRender)
            {
                if (!_npcRendererProvider.NPCRenderers.ContainsKey(index) ||
                    _npcRendererProvider.NPCRenderers[index] == null)
                    throw new InvalidOperationException(
                        $"NPC renderer for ID {index} is null or missing! Did you call MapRenderer.Update() before calling MapRenderer.Draw()?");

                var renderer = _npcRendererProvider.NPCRenderers[index];
                renderer.DrawToSpriteBatch(spriteBatch);
            }
        }

        private static bool IsNpcAt(EOLib.Domain.NPC.NPC npc, int row, int col)
        {
            return (npc.IsActing(NPCActionState.Walking) && npc.GetDestinationX() == col && npc.GetDestinationY() == row) ||
                (npc.X == col && npc.Y == row);
        }
    }
}
