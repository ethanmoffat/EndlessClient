// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Character;
using EOLib.Domain.Map;

namespace EndlessClient.Rendering.Map
{
    public class MapChangedActions : IMapChangedNotifier
    {
        private readonly ICharacterStateCache _characterStateCache;
        private readonly ICharacterRendererRepository _characterRendererRepository;

        public MapChangedActions(ICharacterStateCache characterStateCache,
                                 ICharacterRendererRepository characterRendererRepository)
        {
            _characterStateCache = characterStateCache;
            _characterRendererRepository = characterRendererRepository;
        }

        public void NotifyMapChanged(WarpAnimation animation)
        {
            _characterStateCache.ClearAllOtherCharacterStates();

            foreach (var characterRenderer in _characterRendererRepository.CharacterRenderers)
                characterRenderer.Value.Dispose();
            _characterRendererRepository.CharacterRenderers.Clear();
        }
    }
}
