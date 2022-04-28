using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Domain.NPC;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class RefreshMapStateHandler : InGameOnlyPacketHandler
    {
        private readonly IPacketTranslator<RefreshReplyData> _refreshReplyTranslator;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IMapChangedNotifier> _mapChangedNotifiers;

        public override PacketFamily Family => PacketFamily.Refresh;

        public override PacketAction Action => PacketAction.Reply;

        public RefreshMapStateHandler(IPlayerInfoProvider playerInfoProvider,
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

            var updatedMainCharacter = data.Characters.Single(IDMatches);
            var updatedRenderProperties = _characterRepository.MainCharacter.RenderProperties
                .WithMapX(updatedMainCharacter.RenderProperties.MapX)
                .WithMapY(updatedMainCharacter.RenderProperties.MapY);

            var withoutMainCharacter = data.Characters.Where(x => !IDMatches(x));
            data = data.WithCharacters(withoutMainCharacter.ToList());

            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithRenderProperties(updatedRenderProperties);

            _currentMapStateRepository.Characters = data.Characters.ToDictionary(k => k.ID, v => v);
            _currentMapStateRepository.NPCs = new HashSet<NPC>(data.NPCs);
            _currentMapStateRepository.MapItems = new HashSet<MapItem>(data.Items);

            _currentMapStateRepository.OpenDoors.Clear();
            _currentMapStateRepository.PendingDoors.Clear();
            _currentMapStateRepository.VisibleSpikeTraps.Clear();

            foreach (var notifier in _mapChangedNotifiers)
                notifier.NotifyMapChanged(differentMapID: false, warpAnimation: WarpAnimation.None);

            return true;
        }

        private bool IDMatches(Character x)
        {
            return x.ID == _characterRepository.MainCharacter.ID;
        }
    }
}
