// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Chat;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Localization;

namespace EndlessClient.Rendering.Map
{
    public class MapChangedActions : IMapChangedNotifier
    {
        private readonly ICharacterStateCache _characterStateCache;
        private readonly INPCStateCache _npcStateCache;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly INPCRendererRepository _npcRendererRepository;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringService _localizedStringService;
        private readonly ICurrentMapProvider _currentMapProvider;

        public MapChangedActions(ICharacterStateCache characterStateCache,
                                 INPCStateCache npcStateCache,
                                 ICharacterRendererRepository characterRendererRepository,
                                 INPCRendererRepository npcRendererRepository,
                                 IHudControlProvider hudControlProvider,
                                 IChatRepository chatRepository,
                                 ILocalizedStringService localizedStringService,
                                 ICurrentMapProvider currentMapProvider)
        {
            _characterStateCache = characterStateCache;
            _npcStateCache = npcStateCache;
            _characterRendererRepository = characterRendererRepository;
            _npcRendererRepository = npcRendererRepository;
            _hudControlProvider = hudControlProvider;
            _chatRepository = chatRepository;
            _localizedStringService = localizedStringService;
            _currentMapProvider = currentMapProvider;
        }

        public void NotifyMapChanged(WarpAnimation warpAnimation, bool showMapTransition)
        {
            StopAllAnimations();
            ClearCharacterRenderersAndCache();
            ClearNPCRenderersAndCache();
            ShowMapNameIfAvailable();
            ShowMapTransition(showMapTransition);

            //todo: render warp animation on main character renderer
        }

        private void StopAllAnimations()
        {
            var characterAnimator = _hudControlProvider.GetComponent<ICharacterAnimator>(HudControlIdentifier.CharacterAnimator);
            characterAnimator.StopAllOtherCharacterAnimations();

            var npcAnimator = _hudControlProvider.GetComponent<INPCAnimator>(HudControlIdentifier.NPCAnimator);
            npcAnimator.StopAllAnimations();
        }

        private void ClearCharacterRenderersAndCache()
        {
            foreach (var characterRenderer in _characterRendererRepository.CharacterRenderers)
                characterRenderer.Value.Dispose();
            _characterRendererRepository.CharacterRenderers.Clear();
            _characterStateCache.ClearAllOtherCharacterStates();
        }

        private void ClearNPCRenderersAndCache()
        {
            foreach (var npcRenderer in _npcRendererRepository.NPCRenderers)
                npcRenderer.Value.Dispose();
            _npcRendererRepository.NPCRenderers.Clear();
            _npcStateCache.Reset();
        }

        private void ShowMapNameIfAvailable()
        {
            if (string.IsNullOrWhiteSpace(_currentMapProvider.CurrentMap.Properties.Name))
                return;

            var message = _localizedStringService.GetString(EOResourceID.STATUS_LABEL_YOU_ENTERED);
            message = string.Format(message + " {0}", _currentMapProvider.CurrentMap.Properties.Name);

            var chatData = new ChatData(string.Empty, message, ChatIcon.NoteLeftArrow);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);
        }

        private void ShowMapTransition(bool showMapTransition)
        {
            if (showMapTransition)
            {
                var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
                mapRenderer.StartMapTransition();
            }
        }
    }
}
