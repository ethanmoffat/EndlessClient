using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Input;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Chat;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.Localization;

namespace EndlessClient.Rendering.Map
{
    [MappedType(BaseType = typeof(IMapChangedActions))]
    [MappedType(BaseType = typeof(IMapChangedNotifier))]
    public class MapChangedActions : IMapChangedNotifier, IMapChangedActions
    {
        private readonly ICharacterStateCache _characterStateCache;
        private readonly INPCStateCache _npcStateCache;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly INPCRendererRepository _npcRendererRepository;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public MapChangedActions(ICharacterStateCache characterStateCache,
                                 INPCStateCache npcStateCache,
                                 ICharacterRendererRepository characterRendererRepository,
                                 INPCRendererRepository npcRendererRepository,
                                 IHudControlProvider hudControlProvider,
                                 IChatRepository chatRepository,
                                 ILocalizedStringFinder localizedStringFinder,
                                 ICurrentMapProvider currentMapProvider,
                                 ICurrentMapStateRepository currentMapStateRepository)
        {
            _characterStateCache = characterStateCache;
            _npcStateCache = npcStateCache;
            _characterRendererRepository = characterRendererRepository;
            _npcRendererRepository = npcRendererRepository;
            _hudControlProvider = hudControlProvider;
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
            _currentMapProvider = currentMapProvider;
            _currentMapStateRepository = currentMapStateRepository;
        }

        public void ActiveCharacterEnterMapForLogin()
        {
            ShowMapNameIfAvailable(true);
            ShowMapTransition(true);
            //todo: show message if map is a PK map
        }

        public void NotifyMapChanged(WarpAnimation warpAnimation, bool differentMapID)
        {
            StopAllAnimations();
            ClearCharacterRenderersAndCache();
            ClearNPCRenderersAndCache();
            ShowMapNameIfAvailable(differentMapID);
            //todo: show message if map is a PK map
            ShowMapTransition(differentMapID);
            AddSpikeTraps();
            ShowWarpBubbles(warpAnimation);
        }

        private void StopAllAnimations()
        {
            var characterAnimator = _hudControlProvider.GetComponent<ICharacterAnimator>(HudControlIdentifier.CharacterAnimator);
            characterAnimator.StopAllCharacterAnimations();

            var npcAnimator = _hudControlProvider.GetComponent<INPCAnimator>(HudControlIdentifier.NPCAnimator);
            npcAnimator.StopAllAnimations();

            var clickWalkPathHandler = _hudControlProvider.GetComponent<IClickWalkPathHandler>(HudControlIdentifier.ClickWalkPathHandler);
            clickWalkPathHandler.CancelWalking();
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

        private void ShowMapNameIfAvailable(bool differentMapID)
        {
            if (!differentMapID || string.IsNullOrWhiteSpace(_currentMapProvider.CurrentMap.Properties.Name))
                return;

            var message = _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_YOU_ENTERED);
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

        private void AddSpikeTraps()
        {
            foreach (var character in _currentMapStateRepository.Characters.Values)
            {
                if (_currentMapProvider.CurrentMap.Tiles[character.RenderProperties.MapY, character.RenderProperties.MapX] == TileSpec.SpikesTrap)
                    _currentMapStateRepository.VisibleSpikeTraps.Add(new MapCoordinate(character.RenderProperties.MapX, character.RenderProperties.MapY));
            }
        }

        private void ShowWarpBubbles(WarpAnimation animation)
        {
            if (animation == WarpAnimation.Admin)
            {
                _characterRendererRepository.MainCharacterRenderer.ShowWarpArrive();
            }
        }
    }

    public interface IMapChangedActions
    {
        void ActiveCharacterEnterMapForLogin();
    }
}
