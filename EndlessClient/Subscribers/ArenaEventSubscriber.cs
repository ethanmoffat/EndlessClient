using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Chat;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.Localization;

namespace EndlessClient.Subscribers
{
    [AutoMappedType]
    public class ArenaEventSubscriber : IArenaNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IServerMessageHandler _serverMessageHandler;

        public ArenaEventSubscriber(IHudControlProvider hudControlProvider,
                                    ICurrentMapProvider currentMapProvider,
                                    ICharacterProvider characterProvider,
                                    IChatRepository chatRepository,
                                    ILocalizedStringFinder localizedStringFinder,
                                    IServerMessageHandler serverMessageHandler)
        {
            _hudControlProvider = hudControlProvider;
            _currentMapProvider = currentMapProvider;
            _characterProvider = characterProvider;
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
            _serverMessageHandler = serverMessageHandler;
        }

        public void NotifyArenaBusy()
        {
            var message = _localizedStringFinder.GetString(EOResourceID.ARENA_ROUND_DELAYED_STILL_PLAYERS);
            _serverMessageHandler.AddServerMessage(message, SoundEffectID.ArenaTickSound);
        }

        public void NotifyArenaStart(int players)
        {
            var message = _localizedStringFinder.GetString(EOResourceID.ARENA_PLAYERS_LAUNCHED);
            _serverMessageHandler.AddServerMessage($"{players}{message}");

            var coord = _characterProvider.MainCharacter.RenderProperties.Coordinates();
            if (AdjacentToArenaTile(coord, _currentMapProvider.CurrentMap.Tiles))
            {
                var periodicEmoter = _hudControlProvider.GetComponent<IPeriodicEmoteHandler>(HudControlIdentifier.PeriodicEmoteHandler);
                periodicEmoter.StartArenaBlockTimer();
            }
        }

        public void NotifyArenaKill(int killCount, string killer, string victim)
        {
            var message = $"{victim} {_localizedStringFinder.GetString(EOResourceID.ARENA_WAS_ELIMINATED_BY)}{killer}";

            if (killCount > 1)
            {
                var killed = _localizedStringFinder.GetString(EOResourceID.ARENA_KILLED);
                var players = _localizedStringFinder.GetString(EOResourceID.ARENA_PLAYERS);
                message = $"{message}, {killer} {killed}{killCount}{players}";
            }

            var chatData = new ChatData(ChatTab.System, string.Empty, message, ChatIcon.Skeleton, log: false, filter: false);
            _chatRepository.AllChat[ChatTab.System].Add(chatData);
        }

        public void NotifyArenaWin(string winner)
        {
            var message = _localizedStringFinder.GetString(EOResourceID.ARENA_WON_EVENT);
            _serverMessageHandler.AddServerMessage($"{winner}{message}", SoundEffectID.ArenaWin, ChatIcon.Trophy);
        }

        private static bool AdjacentToArenaTile(MapCoordinate coord, IReadOnlyMatrix<TileSpec> tiles)
        {
            var check = new[]
            {
                coord,
                new MapCoordinate(coord.X - 1, coord.Y),
                new MapCoordinate(coord.X, coord.Y - 1),
                new MapCoordinate(coord.X + 1, coord.Y),
                new MapCoordinate(coord.X, coord.Y + 1),
            };

            foreach (var checkCoord in check)
            {
                if (checkCoord.X >= 0 && checkCoord.X <= tiles.Cols &&
                    checkCoord.Y >= 0 && checkCoord.Y <= tiles.Rows &&
                    tiles[checkCoord.Y, checkCoord.X] == TileSpec.Arena)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
