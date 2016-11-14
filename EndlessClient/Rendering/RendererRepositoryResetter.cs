// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.NPC;

namespace EndlessClient.Rendering
{
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
