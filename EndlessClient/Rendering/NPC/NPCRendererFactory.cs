using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.NPC;
using EOLib.Graphics;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.NPC
{
    [MappedType(BaseType = typeof(INPCRendererFactory))]
    public class NPCRendererFactory : INPCRendererFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly INPCSpriteSheet _npcSpriteSheet;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;

        public NPCRendererFactory(INativeGraphicsManager nativeGraphicsManager,
                                  IEndlessGameProvider endlessGameProvider,
                                  ICharacterRendererProvider characterRendererProvider,
                                  IENFFileProvider enfFileProvider,
                                  INPCSpriteSheet npcSpriteSheet,
                                  IRenderOffsetCalculator renderOffsetCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _endlessGameProvider = endlessGameProvider;
            _characterRendererProvider = characterRendererProvider;
            _enfFileProvider = enfFileProvider;
            _npcSpriteSheet = npcSpriteSheet;
            _renderOffsetCalculator = renderOffsetCalculator;
        }

        public INPCRenderer CreateRendererFor(INPC npc)
        {
            return new NPCRenderer(_nativeGraphicsManager,
                                   _endlessGameProvider,
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
