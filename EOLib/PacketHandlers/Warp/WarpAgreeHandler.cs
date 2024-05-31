using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;

using DomainNPC = EOLib.Domain.NPC.NPC;

namespace EOLib.PacketHandlers.Warp
{
    /// <summary>
    /// Sent when completing warp for the main character
    /// </summary>
    [AutoMappedType]
    public class WarpAgreeHandler : InGameOnlyPacketHandler<WarpAgreeServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IEnumerable<IMapChangedNotifier> _mapChangedNotifiers;

        public override PacketFamily Family => PacketFamily.Warp;

        public override PacketAction Action => PacketAction.Agree;

        public WarpAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapStateRepository currentMapStateRepository,
                                ICurrentMapProvider currentMapProvider,
                                IEIFFileProvider eifFileProvider,
                                IEnumerable<IMapChangedNotifier> mapChangedNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _currentMapProvider = currentMapProvider;
            _eifFileProvider = eifFileProvider;
            _mapChangedNotifiers = mapChangedNotifiers;
        }

        public override bool HandlePacket(WarpAgreeServerPacket packet)
        {
            _currentMapStateRepository.MapWarpState = WarpState.WarpCompleting;

            var differentMapID = false;
            var warpAnimation = WarpEffect.None;
            if (packet.WarpType == WarpType.MapSwitch)
            {
                var data = (WarpAgreeServerPacket.WarpTypeDataMapSwitch)packet.WarpTypeData;
                differentMapID = _currentMapStateRepository.CurrentMapID != data.MapId;
                warpAnimation = data.WarpEffect;
                _currentMapStateRepository.CurrentMapID = data.MapId;
            }

            var characters = packet.Nearby.Characters.Select(Character.FromNearby).ToList();
            var updatedMainCharacter = characters.Single(x => x.ID == _characterRepository.MainCharacter.ID);
            var withoutMainCharacter = characters.Except(new[] { updatedMainCharacter });

            var npcs = packet.Nearby.Npcs.Select(DomainNPC.FromNearby);
            var items = packet.Nearby.Items.Select(MapItem.FromNearby);

            //character.renderproperties.isdead is set True by the attack handler
            //the character needs to be brought back to life when they are taken to the home map
            var bringBackToLife = _characterRepository.MainCharacter.RenderProperties.WithIsDead(false);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithRenderProperties(bringBackToLife)
                .WithAppliedData(updatedMainCharacter);

            _currentMapStateRepository.Characters = new MapEntityCollectionHashSet<Character>(c => c.ID, c => new MapCoordinate(c.X, c.Y), withoutMainCharacter);
            _currentMapStateRepository.NPCs = new MapEntityCollectionHashSet<DomainNPC>(n => n.Index, n => new MapCoordinate(n.X, n.Y), npcs);
            _currentMapStateRepository.MapItems = new MapEntityCollectionHashSet<MapItem>(item => item.UniqueID, item => new MapCoordinate(item.X, item.Y), items);
            _currentMapStateRepository.OpenDoors.Clear();
            _currentMapStateRepository.PendingDoors.Clear();
            _currentMapStateRepository.ShowMiniMap = _currentMapStateRepository.ShowMiniMap &&
                                                     _currentMapProvider.CurrentMap.Properties.MapAvailable;

            foreach (var notifier in _mapChangedNotifiers)
                notifier.NotifyMapChanged(warpAnimation, differentMapID);

            _currentMapStateRepository.MapWarpState = WarpState.None;
            _currentMapStateRepository.MapWarpTime = Option.Some(DateTime.Now);

            return true;
        }
    }
}
