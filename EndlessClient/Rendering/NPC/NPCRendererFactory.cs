using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
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
        private readonly IHealthBarRendererFactory _healthBarRendererFactory;

        public NPCRendererFactory(INativeGraphicsManager nativeGraphicsManager,
                                  IEndlessGameProvider endlessGameProvider,
                                  ICharacterRendererProvider characterRendererProvider,
                                  IENFFileProvider enfFileProvider,
                                  INPCSpriteSheet npcSpriteSheet,
                                  IRenderOffsetCalculator renderOffsetCalculator,
                                  IHealthBarRendererFactory healthBarRendererFactory)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _endlessGameProvider = endlessGameProvider;
            _characterRendererProvider = characterRendererProvider;
            _enfFileProvider = enfFileProvider;
            _npcSpriteSheet = npcSpriteSheet;
            _renderOffsetCalculator = renderOffsetCalculator;
            _healthBarRendererFactory = healthBarRendererFactory;
        }

        public INPCRenderer CreateRendererFor(INPC npc)
        {
            return new NPCRenderer(_nativeGraphicsManager,
                                   _endlessGameProvider,
                                   _characterRendererProvider,
                                   _enfFileProvider,
                                   _npcSpriteSheet,
                                   _renderOffsetCalculator,
                                   _healthBarRendererFactory,
                                   npc);
        }
    }

    public interface INPCRendererFactory
    {
        INPCRenderer CreateRendererFor(INPC npc);
    }
}
