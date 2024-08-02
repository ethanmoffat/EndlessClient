using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.NPC;
using EndlessClient.Rendering.Sprites;
using EOLib.Config;
using EOLib.Domain.Chat;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EndlessClient.Rendering.Map
{
    [MappedType(BaseType = typeof(IMapChangedActions))]
    [MappedType(BaseType = typeof(IMapChangedNotifier))]
    public class MapChangedActions : IMapChangedNotifier, IMapChangedActions
    {
        private readonly ICharacterStateCache _characterStateCache;
        private readonly INPCStateCache _npcStateCache;
        private readonly INPCSpriteDataCache _npcSpriteDataCache;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly INPCRendererRepository _npcRendererRepository;
        private readonly IENFFileProvider _enfFileProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IActiveDialogProvider _dialogProvider;
        private readonly IMfxPlayer _mfxPlayer;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IChatController _chatController;

        public MapChangedActions(ICharacterStateCache characterStateCache,
                                 INPCStateCache npcStateCache,
                                 INPCSpriteDataCache npcSpriteDataCache,
                                 ICharacterRendererRepository characterRendererRepository,
                                 INPCRendererRepository npcRendererRepository,
                                 IENFFileProvider enfFileProvider,
                                 IHudControlProvider hudControlProvider,
                                 IChatRepository chatRepository,
                                 ILocalizedStringFinder localizedStringFinder,
                                 ICurrentMapProvider currentMapProvider,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 IConfigurationProvider configurationProvider,
                                 IActiveDialogProvider dialogProvider,
                                 IMfxPlayer mfxPlayer,
                                 ISfxPlayer sfxPlayer,
                                 IChatController chatController)
        {
            _characterStateCache = characterStateCache;
            _npcStateCache = npcStateCache;
            _npcSpriteDataCache = npcSpriteDataCache;
            _characterRendererRepository = characterRendererRepository;
            _npcRendererRepository = npcRendererRepository;
            _enfFileProvider = enfFileProvider;
            _hudControlProvider = hudControlProvider;
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
            _currentMapProvider = currentMapProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _configurationProvider = configurationProvider;
            _dialogProvider = dialogProvider;
            _mfxPlayer = mfxPlayer;
            _sfxPlayer = sfxPlayer;
            _chatController = chatController;
        }

        public void ActiveCharacterEnterMapForLogin()
        {
            ShowMapNameIfAvailable(differentMapID: true);
            ShowMapTransition(showMapTransition: true);
            PlayBackgroundMusic(differentMapID: true);
            PlayAmbientNoise(differentMapID: true);
            ShowPkWarning(differentMapId: true);
        }

        public void NotifyMapChanged(WarpEffect warpAnimation, bool differentMapID)
        {
            StopAllAnimations();
            ClearCharacterRenderersAndCache();
            ClearNPCRenderersAndCache();
            ClearOpenDoors();
            ShowMapNameIfAvailable(differentMapID);
            ShowPkWarning(differentMapID);
            ShowMapTransition(differentMapID);
            ShowWarpBubbles(warpAnimation);
            PlayBackgroundMusic(differentMapID);
            PlayAmbientNoise(differentMapID);

            if (!differentMapID)
                RedrawGroundLayer();

            CloseAllDialogs();

            _chatController.ClearAndWarnIfJailAndGlobal();
        }

        public void NotifyMapMutation()
        {
            ClearOpenDoors();

            ShowMapTransition(showMapTransition: true);

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
            var currentMapNpcGraphics = _currentMapStateRepository.NPCs.Select(x => _enfFileProvider.ENFFile[x.ID].Graphic).ToList();
            var priorMapNpcGraphics = _npcRendererRepository.NPCRenderers.Select(x => _enfFileProvider.ENFFile[x.Value.NPC.ID].Graphic);

            foreach (var evict in priorMapNpcGraphics.Except(currentMapNpcGraphics))
                _npcSpriteDataCache.MarkForEviction(evict);

            foreach (var unevict in currentMapNpcGraphics)
                _npcSpriteDataCache.UnmarkForEviction(unevict);

            foreach (var npcRenderer in _npcRendererRepository.NPCRenderers)
                npcRenderer.Value.Dispose();
            _npcRendererRepository.NPCRenderers.Clear();
            _npcStateCache.Reset();
        }

        private void ClearOpenDoors()
        {
            _currentMapStateRepository.OpenDoors.Clear();
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

        private void ShowPkWarning(bool differentMapId)
        {
            if (!differentMapId || !_currentMapProvider.CurrentMap.Properties.PKAvailable)
                return;

            var message = _localizedStringFinder.GetString(EOResourceID.CAUTION_THIS_IS_A_PK_ZONE);
            var chatData = new ChatData(ChatTab.System, string.Empty, message, ChatIcon.NoteLeftArrow);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);

            _sfxPlayer.PlaySfx(SoundEffectID.EnterPkMap);
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

        private void ShowWarpBubbles(WarpEffect animation)
        {
            _characterRendererRepository.MainCharacterRenderer.MatchSome(r =>
            {
                if (animation == WarpEffect.Admin)
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
            {
                _sfxPlayer.StopLoopingSfx();
                _sfxPlayer.PlayLoopingSfx((SoundEffectID)noise);
            }
            else
                _sfxPlayer.StopLoopingSfx();
        }

        private void CloseAllDialogs()
        {
            foreach (var dlg in _dialogProvider.ActiveDialogs)
                dlg.MatchSome(x => ((BaseEODialog)x).Close());
        }
    }

    public interface IMapChangedActions
    {
        void ActiveCharacterEnterMapForLogin();
    }
}