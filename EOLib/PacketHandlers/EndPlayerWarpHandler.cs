// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;

namespace EOLib.PacketHandlers
{
    public class EndPlayerWarpHandler : InGameOnlyPacketHandler
    {
        private readonly IPacketTranslator<IWarpAgreePacketData> _warpAgreePacketTranslator;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IEnumerable<IMapChangedNotifier> _mapChangedNotifiers;

        public override PacketFamily Family { get { return PacketFamily.Warp; } }

        public override PacketAction Action { get { return PacketAction.Agree; } }

        public EndPlayerWarpHandler(IPlayerInfoProvider playerInfoProvider,
                                    IPacketTranslator<IWarpAgreePacketData> warpAgreePacketTranslator,
                                    ICharacterRepository characterRepository,
                                    ICurrentMapStateRepository currentMapStateRepository,
                                    ICurrentMapProvider currentMapProvider,
                                    IEnumerable<IMapChangedNotifier> mapChangedNotifiers)
            : base(playerInfoProvider)
        {
            _warpAgreePacketTranslator = warpAgreePacketTranslator;
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _currentMapProvider = currentMapProvider;
            _mapChangedNotifiers = mapChangedNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var warpAgreePacketData = _warpAgreePacketTranslator.TranslatePacket(packet);

            var updatedMainCharacter = warpAgreePacketData.Characters.Single(NameMatches);
            var updatedRenderProperties = _characterRepository.MainCharacter.RenderProperties
                .WithMapX(updatedMainCharacter.RenderProperties.MapX)
                .WithMapY(updatedMainCharacter.RenderProperties.MapY);

            var withoutMainCharacter = warpAgreePacketData.Characters.Where(x => !NameMatches(x));
            warpAgreePacketData = warpAgreePacketData.WithCharacters(withoutMainCharacter);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithMapID(warpAgreePacketData.MapID)
                .WithRenderProperties(updatedRenderProperties);

            _currentMapStateRepository.CurrentMapID = warpAgreePacketData.MapID;
            _currentMapStateRepository.Characters = warpAgreePacketData.Characters.ToList();
            _currentMapStateRepository.NPCs = warpAgreePacketData.NPCs.ToList();
            _currentMapStateRepository.MapItems = warpAgreePacketData.Items.ToList();
            _currentMapStateRepository.OpenDoors.Clear();
            _currentMapStateRepository.ShowMiniMap = _currentMapStateRepository.ShowMiniMap &&
                                                     _currentMapProvider.CurrentMap.Properties.MapAvailable;

            foreach (var notifier in _mapChangedNotifiers)
                notifier.NotifyMapChanged(showMapTransition: true, //todo: only show transition if its a different map
                                          warpAnimation: warpAgreePacketData.WarpAnimation);

            return true;
        }

        private bool NameMatches(ICharacter x)
        {
            //todo: figure out why ID isn't matching
            return x.Name.ToLower() == _characterRepository.MainCharacter.Name.ToLower();
        }
    }
}
