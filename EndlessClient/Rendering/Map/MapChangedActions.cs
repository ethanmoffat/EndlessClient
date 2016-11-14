// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Map;

namespace EndlessClient.Rendering.Map
{
    public class MapChangedActions : IMapChangedNotifier
    {
        private readonly ICharacterStateCache _characterStateCache;
        private readonly INPCStateCache _npcStateCache;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly INPCRendererRepository _npcRendererRepository;
        private readonly IHudControlProvider _hudControlProvider;

        public MapChangedActions(ICharacterStateCache characterStateCache,
                                 INPCStateCache npcStateCache,
                                 ICharacterRendererRepository characterRendererRepository,
                                 INPCRendererRepository npcRendererRepository,
                                 IHudControlProvider hudControlProvider)
        {
            _characterStateCache = characterStateCache;
            _npcStateCache = npcStateCache;
            _characterRendererRepository = characterRendererRepository;
            _npcRendererRepository = npcRendererRepository;
            _hudControlProvider = hudControlProvider;
        }

        public void NotifyMapChanged(WarpAnimation warpAnimation, bool showMapTransition)
        {
            foreach (var characterRenderer in _characterRendererRepository.CharacterRenderers)
                characterRenderer.Value.Dispose();
            _characterRendererRepository.CharacterRenderers.Clear();
            _characterStateCache.ClearAllOtherCharacterStates();

            foreach (var npcRenderer in _npcRendererRepository.NPCRenderers)
                npcRenderer.Value.Dispose();
            _npcRendererRepository.NPCRenderers.Clear();
            _npcStateCache.Reset();

            if (showMapTransition)
            {
                var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
                mapRenderer.StartMapTransition();
            }

            //todo: render warp animation on main character renderer
        }
    }
}
