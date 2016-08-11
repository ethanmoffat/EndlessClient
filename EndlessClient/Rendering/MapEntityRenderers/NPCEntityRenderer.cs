// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class NPCEntityRenderer : BaseMapEntityRenderer
    {
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        public NPCEntityRenderer(ICharacterProvider characterProvider,
                                 ICurrentMapStateProvider currentMapStateProvider,
                                 ICharacterRenderOffsetCalculator characterRenderOffsetCalculator)
            : base(characterProvider, characterRenderOffsetCalculator)
        {
            _currentMapStateProvider = currentMapStateProvider;
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
            //todo: render NPCs when NPCs are supported
        }
    }
}
