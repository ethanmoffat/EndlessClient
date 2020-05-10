using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class EndPlayerWarpHandler : InGameOnlyPacketHandler
    {
        private readonly IPacketTranslator<IWarpAgreePacketData> _warpAgreePacketTranslator;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IEnumerable<IMapChangedNotifier> _mapChangedNotifiers;

        public override PacketFamily Family => PacketFamily.Warp;

        public override PacketAction Action => PacketAction.Agree;

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
            _currentMapStateRepository.MapWarpState = WarpState.WarpCompleting;

            var warpAgreePacketData = _warpAgreePacketTranslator.TranslatePacket(packet);

            var updatedMainCharacter = warpAgreePacketData.Characters.Single(MainCharacterIDMatches);

            //character.renderproperties.isdead is set True by the attack handler
            //the character needs to be brought back to life when the are taken to the home map
            var bringBackToLife = _characterRepository.MainCharacter.RenderProperties.WithAlive();
            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithRenderProperties(bringBackToLife)
                .WithAppliedData(updatedMainCharacter);

            var withoutMainCharacter = warpAgreePacketData.Characters.Where(x => !MainCharacterIDMatches(x));
            warpAgreePacketData = warpAgreePacketData.WithCharacters(withoutMainCharacter);

            var differentMapID = _currentMapStateRepository.CurrentMapID != warpAgreePacketData.MapID;

            _currentMapStateRepository.CurrentMapID = warpAgreePacketData.MapID;
            _currentMapStateRepository.Characters = warpAgreePacketData.Characters.ToList();
            _currentMapStateRepository.NPCs = warpAgreePacketData.NPCs.ToList();
            _currentMapStateRepository.MapItems = warpAgreePacketData.Items.ToList();
            _currentMapStateRepository.OpenDoors.Clear();
            _currentMapStateRepository.ShowMiniMap = _currentMapStateRepository.ShowMiniMap &&
                                                     _currentMapProvider.CurrentMap.Properties.MapAvailable;

            foreach (var notifier in _mapChangedNotifiers)
                notifier.NotifyMapChanged(differentMapID: differentMapID,
                                          warpAnimation: warpAgreePacketData.WarpAnimation);

            _currentMapStateRepository.MapWarpState = WarpState.None;

            return true;
        }

        private bool MainCharacterIDMatches(ICharacter x)
        {
            return x.ID == _characterRepository.MainCharacter.ID;
        }
    }
}
