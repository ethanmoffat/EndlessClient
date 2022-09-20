using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Domain.NPC;
using EOLib.IO.Extensions;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class EndPlayerWarpHandler : InGameOnlyPacketHandler
    {
        private readonly IPacketTranslator<WarpAgreePacketData> _warpAgreePacketTranslator;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IEnumerable<IMapChangedNotifier> _mapChangedNotifiers;

        public override PacketFamily Family => PacketFamily.Warp;

        public override PacketAction Action => PacketAction.Agree;

        public EndPlayerWarpHandler(IPlayerInfoProvider playerInfoProvider,
                                    IPacketTranslator<WarpAgreePacketData> warpAgreePacketTranslator,
                                    ICharacterRepository characterRepository,
                                    ICurrentMapStateRepository currentMapStateRepository,
                                    ICurrentMapProvider currentMapProvider,
                                    IEIFFileProvider eifFileProvider,
                                    IEnumerable<IMapChangedNotifier> mapChangedNotifiers)
            : base(playerInfoProvider)
        {
            _warpAgreePacketTranslator = warpAgreePacketTranslator;
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _currentMapProvider = currentMapProvider;
            _eifFileProvider = eifFileProvider;
            _mapChangedNotifiers = mapChangedNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            _currentMapStateRepository.MapWarpState = WarpState.WarpCompleting;

            var warpAgreePacketData = _warpAgreePacketTranslator.TranslatePacket(packet);

            _currentMapStateRepository.CurrentMapID = warpAgreePacketData.MapID;

            var updatedMainCharacter = warpAgreePacketData.Characters.Single(MainCharacterIDMatches);

            //character.renderproperties.isdead is set True by the attack handler
            //the character needs to be brought back to life when they are taken to the home map
            var bringBackToLife = _characterRepository.MainCharacter.RenderProperties.WithIsDead(false);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithRenderProperties(bringBackToLife)
                .WithAppliedData(updatedMainCharacter, _eifFileProvider.EIFFile.IsRangedWeapon(updatedMainCharacter.RenderProperties.WeaponGraphic));

            var withoutMainCharacter = warpAgreePacketData.Characters.Where(x => !MainCharacterIDMatches(x));
            warpAgreePacketData = warpAgreePacketData.WithCharacters(withoutMainCharacter.ToList());

            var differentMapID = _currentMapStateRepository.CurrentMapID != warpAgreePacketData.MapID;

            _currentMapStateRepository.Characters = warpAgreePacketData.Characters.ToDictionary(k => k.ID, v => v);
            _currentMapStateRepository.NPCs = new HashSet<NPC>(warpAgreePacketData.NPCs);
            _currentMapStateRepository.MapItems = new HashSet<MapItem>(warpAgreePacketData.Items);
            _currentMapStateRepository.OpenDoors.Clear();
            _currentMapStateRepository.VisibleSpikeTraps.Clear();
            _currentMapStateRepository.ShowMiniMap = _currentMapStateRepository.ShowMiniMap &&
                                                     _currentMapProvider.CurrentMap.Properties.MapAvailable;

            foreach (var notifier in _mapChangedNotifiers)
                notifier.NotifyMapChanged(differentMapID: differentMapID,
                                          warpAnimation: warpAgreePacketData.WarpAnimation);

            _currentMapStateRepository.MapWarpState = WarpState.None;
            _currentMapStateRepository.MapWarpTime = Option.Some(DateTime.Now);

            return true;
        }

        private bool MainCharacterIDMatches(Character x)
        {
            return x.ID == _characterRepository.MainCharacter.ID;
        }
    }
}
