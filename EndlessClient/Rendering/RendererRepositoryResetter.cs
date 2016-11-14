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

        public RendererRepositoryResetter(ICharacterRendererRepository characterRendererRepository,
                                          INPCRendererRepository npcRendererRepository)
        {
            _characterRendererRepository = characterRendererRepository;
            _npcRendererRepository = npcRendererRepository;
        }

        public void ResetRenderers()
        {
            _characterRendererRepository.Dispose();
            _npcRendererRepository.Dispose();
        }
    }

    public interface IRendererRepositoryResetter
    {
        void ResetRenderers();
    }
}
