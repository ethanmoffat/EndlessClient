using AutomaticTypeMapper;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.NPC;

namespace EndlessClient.Rendering
{
    [MappedType(BaseType = typeof(IRendererRepositoryResetter))]
    public class RendererRepositoryResetter : IRendererRepositoryResetter
    {
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly INPCRendererRepository _npcRendererRepository;
        private readonly ICharacterStateCache _characterStateCache;
        private readonly INPCStateCache _npcStateCache;

        public RendererRepositoryResetter(ICharacterRendererRepository characterRendererRepository,
                                          INPCRendererRepository npcRendererRepository,
                                          ICharacterStateCache characterStateCache,
                                          INPCStateCache npcStateCache)
        {
            _characterRendererRepository = characterRendererRepository;
            _npcRendererRepository = npcRendererRepository;
            _characterStateCache = characterStateCache;
            _npcStateCache = npcStateCache;
        }

        public void ResetRenderers()
        {
            _characterRendererRepository.Dispose();
            _npcRendererRepository.Dispose();

            _characterStateCache.Reset();
            _npcStateCache.Reset();
        }
    }

    public interface IRendererRepositoryResetter
    {
        void ResetRenderers();
    }
}