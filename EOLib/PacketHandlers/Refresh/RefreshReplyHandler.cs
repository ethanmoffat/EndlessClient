using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using DomainNPC = EOLib.Domain.NPC.NPC;

namespace EOLib.PacketHandlers.Refresh
{
    /// <summary>
    /// Sent when the map state (characters, npcs, and map items) should be refreshed
    /// </summary>
    [AutoMappedType]
    public class RefreshReplyHandler : InGameOnlyPacketHandler<RefreshReplyServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IMapChangedNotifier> _mapChangedNotifiers;

        public override PacketFamily Family => PacketFamily.Refresh;

        public override PacketAction Action => PacketAction.Reply;

        public RefreshReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICharacterRepository characterRepository,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   IEnumerable<IMapChangedNotifier> mapChangedNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _mapChangedNotifiers = mapChangedNotifiers;
        }

        public override bool HandlePacket(RefreshReplyServerPacket packet)
        {
            var characters = packet.Nearby.Characters
                .Where(x => x.ByteSize >= 42)
                .Select(Character.FromNearby)
                .ToList();

            var updatedMainCharacter = characters.Single(MainCharacterIDMatches);
            var updatedRenderProperties = _characterRepository.MainCharacter.RenderProperties
                .WithMapX(updatedMainCharacter.RenderProperties.MapX)
                .WithMapY(updatedMainCharacter.RenderProperties.MapY);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithRenderProperties(updatedRenderProperties);

            var withoutMainCharacter = characters.Where(x => !MainCharacterIDMatches(x));
            _currentMapStateRepository.Characters = new MapEntityCollectionHashSet<Character>(c => c.ID, c => new MapCoordinate(c.X, c.Y), withoutMainCharacter);
            _currentMapStateRepository.NPCs = new MapEntityCollectionHashSet<DomainNPC>(n => n.Index, n => new MapCoordinate(n.X, n.Y), packet.Nearby.Npcs.Select(DomainNPC.FromNearby));
            _currentMapStateRepository.MapItems = new MapEntityCollectionHashSet<MapItem>(item => item.UniqueID, item => new MapCoordinate(item.X, item.Y), packet.Nearby.Items.Select(MapItem.FromNearby));

            _currentMapStateRepository.OpenDoors.Clear();
            _currentMapStateRepository.PendingDoors.Clear();

            _currentMapStateRepository.MapWarpTime = Optional.Option.Some(System.DateTime.Now.AddMilliseconds(-100));

            foreach (var notifier in _mapChangedNotifiers)
                notifier.NotifyMapChanged(WarpEffect.None, differentMapID: false);

            return true;
        }

        private bool MainCharacterIDMatches(Character x)
        {
            return x.ID == _characterRepository.MainCharacter.ID;
        }
    }
}
