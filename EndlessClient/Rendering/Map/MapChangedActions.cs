using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.NPC;
using EOLib.Config;
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
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IMfxPlayer _mfxPlayer;
        private readonly ISfxPlayer _sfxPlayer;

        public MapChangedActions(ICharacterStateCache characterStateCache,
                                 INPCStateCache npcStateCache,
                                 ICharacterRendererRepository characterRendererRepository,
                                 INPCRendererRepository npcRendererRepository,
                                 IHudControlProvider hudControlProvider,
                                 IChatRepository chatRepository,
                                 ILocalizedStringFinder localizedStringFinder,
                                 ICurrentMapProvider currentMapProvider,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 IConfigurationProvider configurationProvider,
                                 IMfxPlayer mfxPlayer,
                                 ISfxPlayer sfxPlayer)
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
            _configurationProvider = configurationProvider;
            _mfxPlayer = mfxPlayer;
            _sfxPlayer = sfxPlayer;
        }

        public void ActiveCharacterEnterMapForLogin()
        {
            ShowMapNameIfAvailable(true);
            ShowMapTransition(true);
            PlayBackgroundMusic(differentMapID: true);
            PlayAmbientNoise(differentMapID: true);
            //todo: show message if map is a PK map
        }

        public void NotifyMapChanged(WarpAnimation warpAnimation, bool differentMapID)
        {
            StopAllAnimations();
            ClearCharacterRenderersAndCache();
            ClearNPCRenderersAndCache();
            ClearOpenDoors();
            ShowMapNameIfAvailable(differentMapID);
            //todo: show message if map is a PK map
            ShowMapTransition(differentMapID);
            AddSpikeTraps();
            ShowWarpBubbles(warpAnimation);
            PlayBackgroundMusic(differentMapID);
            PlayAmbientNoise(differentMapID);

            if (!differentMapID)
                RedrawGroundLayer();
        }

        public void NotifyMapMutation()
        {
            ClearOpenDoors();
            ClearSpikeTraps();

            ShowMapTransition(showMapTransition: true);

            AddSpikeTraps();
            RedrawGroundLayer();

            var localChatData = new ChatData(ChatTab.Local, _localizedStringFinder.GetString(EOResourceID.STRING_SERVER), _localizedStringFinder.GetString(EOResourceID.SERVER_MESSAGE_MAP_MUTATION), ChatIcon.Exclamation, ChatColor.Server);
            var systemChatData = new ChatData(ChatTab.System, _localizedStringFinder.GetString(EOResourceID.STRING_SERVER), _localizedStringFinder.GetString(EOResourceID.SERVER_MESSAGE_MAP_MUTATION), ChatIcon.Exclamation, ChatColor.Server);
            _chatRepository.AllChat[ChatTab.Local].Add(localChatData);
            _chatRepository.AllChat[ChatTab.System].Add(systemChatData);

            _sfxPlayer.PlaySfx(SoundEffectID.MapMutation);
        }

        private void StopAllAnimations()
        {
            var characterAnimator = _hudControlProvider.GetComponent<ICharacterAnimator>(HudControlIdentifier.CharacterAnimator);
            characterAnimator.StopAllCharacterAnimations();

            var npcAnimator = _hudControlProvider.GetComponent<INPCAnimator>(HudControlIdentifier.NPCAnimator);
            npcAnimator.StopAllAnimations();
        }

        private void ClearCharacterRenderersAndCache()
        {
            _characterRendererRepository.MainCharacterRenderer.MatchSome(x => x.StopShout());

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

        private void ClearOpenDoors()
        {
            _currentMapStateRepository.OpenDoors.Clear();
        }

        private void ClearSpikeTraps()
        {
            _currentMapStateRepository.VisibleSpikeTraps.Clear();
        }

        private void ShowMapNameIfAvailable(bool differentMapID)
        {
            if (!differentMapID || string.IsNullOrWhiteSpace(_currentMapProvider.CurrentMap.Properties.Name))
                return;

            var message = _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_YOU_ENTERED);
            message = string.Format(message + " {0}", _currentMapProvider.CurrentMap.Properties.Name);

            var chatData = new ChatData(ChatTab.System, string.Empty, message, ChatIcon.NoteLeftArrow);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);
        }

        private void ShowMapTransition(bool showMapTransition)
        {
            var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
            mapRenderer.ClearTransientRenderables();

            if (showMapTransition)
            {
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
            _characterRendererRepository.MainCharacterRenderer.MatchSome(r =>
            {
                if (animation == WarpAnimation.Admin)
                    r.PlayEffect((int)HardCodedEffect.WarpArrive);
            });
        }

        private void RedrawGroundLayer()
        {
            var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
            mapRenderer.RedrawGroundLayer();
        }

        private void PlayBackgroundMusic(bool differentMapID)
        {
            if (!_configurationProvider.MusicEnabled || !differentMapID)
                return;

            var music = _currentMapProvider.CurrentMap.Properties.Music;
            var musicControl = _currentMapProvider.CurrentMap.Properties.Control;
            if (music > 0)
                _mfxPlayer.PlayBackgroundMusic(_currentMapProvider.CurrentMap.Properties.Music, musicControl);
            else
                _mfxPlayer.StopBackgroundMusic();
        }

        private void PlayAmbientNoise(bool differentMapID)
        {
            if (!_configurationProvider.SoundEnabled || !differentMapID)
                return;

            var noise = _currentMapProvider.CurrentMap.Properties.AmbientNoise;
            if (noise > 0)
                _sfxPlayer.PlayLoopingSfx((SoundEffectID)noise - 1);
            else
                _sfxPlayer.StopLoopingSfx();
        }
    }

    public interface IMapChangedActions
    {
        void ActiveCharacterEnterMapForLogin();
    }
}
