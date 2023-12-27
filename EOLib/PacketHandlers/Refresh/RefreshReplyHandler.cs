using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;
using System.Collections.Generic;
using System.Linq;

using DomainNPC = EOLib.Domain.NPC.NPC;

namespace EOLib.PacketHandlers.Refresh
{
    /// <summary>
    /// Sent when the map state (characters, npcs, and map items) should be refreshed
    /// </summary>
    [AutoMappedType]
    public class RefreshReplyHandler : InGameOnlyPacketHandler
    {
        private readonly IPacketTranslator<RefreshReplyData> _refreshReplyTranslator;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IMapChangedNotifier> _mapChangedNotifiers;

        public override PacketFamily Family => PacketFamily.Refresh;

        public override PacketAction Action => PacketAction.Reply;

        public RefreshReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                   IPacketTranslator<RefreshReplyData> refreshReplyTranslator,
                                   ICharacterRepository characterRepository,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   IEnumerable<IMapChangedNotifier> mapChangedNotifiers)
            : base(playerInfoProvider)
        {
            _refreshReplyTranslator = refreshReplyTranslator;
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _mapChangedNotifiers = mapChangedNotifiers;
        }

        //todo: this is almost identical to EndPlayerWarpHandler - see if there is some way to share
        public override bool HandlePacket(IPacket packet)
        {
            var data = _refreshReplyTranslator.TranslatePacket(packet);

            var updatedMainCharacter = data.Characters.Single(MainCharacterIDMatches);
            var updatedRenderProperties = _characterRepository.MainCharacter.RenderProperties
                .WithMapX(updatedMainCharacter.RenderProperties.MapX)
                .WithMapY(updatedMainCharacter.RenderProperties.MapY);

            var withoutMainCharacter = data.Characters.Where(x => !MainCharacterIDMatches(x));

            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithRenderProperties(updatedRenderProperties);

            _currentMapStateRepository.Characters = new MapEntityCollectionHashSet<Character>(c => c.ID, c => new MapCoordinate(c.X, c.Y),  withoutMainCharacter);
            _currentMapStateRepository.NPCs = new MapEntityCollectionHashSet<DomainNPC>(n => n.Index, n => new MapCoordinate(n.X, n.Y), data.NPCs);
            _currentMapStateRepository.MapItems = new MapEntityCollectionHashSet<MapItem>(item => item.UniqueID, item => new MapCoordinate(item.X, item.Y), data.Items);

            _currentMapStateRepository.OpenDoors.Clear();
            _currentMapStateRepository.PendingDoors.Clear();
            _currentMapStateRepository.VisibleSpikeTraps.Clear();

            _currentMapStateRepository.MapWarpTime = Optional.Option.Some(System.DateTime.Now.AddMilliseconds(-100));

            foreach (var notifier in _mapChangedNotifiers)
                notifier.NotifyMapChanged(differentMapID: false, warpAnimation: WarpAnimation.None);

            return true;
        }

        private bool MainCharacterIDMatches(Character x)
        {
            return x.ID == _characterRepository.MainCharacter.ID;
        }
    }
}
