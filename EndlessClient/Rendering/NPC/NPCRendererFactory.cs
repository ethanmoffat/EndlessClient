// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.NPC;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.NPC
{
    public class NPCRendererFactory : INPCRendererFactory
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly INPCSpriteSheet _npcSpriteSheet;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;

        public NPCRendererFactory(IEndlessGameProvider endlessGameProvider,
                                  ICharacterRendererProvider characterRendererProvider,
                                  IENFFileProvider enfFileProvider,
                                  INPCSpriteSheet npcSpriteSheet,
                                  IRenderOffsetCalculator renderOffsetCalculator)
        {
            _endlessGameProvider = endlessGameProvider;
            _characterRendererProvider = characterRendererProvider;
            _enfFileProvider = enfFileProvider;
            _npcSpriteSheet = npcSpriteSheet;
            _renderOffsetCalculator = renderOffsetCalculator;
        }

        public INPCRenderer CreateRendererFor(INPC npc)
        {
            return new NPCRenderer(_endlessGameProvider,
                                   _characterRendererProvider,
                                   _enfFileProvider,
                                   _npcSpriteSheet,
                                   _renderOffsetCalculator,
                                   npc);
        }
    }

    public interface INPCRendererFactory
    {
        INPCRenderer CreateRendererFor(INPC npc);
    }
}
