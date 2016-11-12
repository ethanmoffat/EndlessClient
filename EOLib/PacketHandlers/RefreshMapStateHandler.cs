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
    public class RefreshMapStateHandler : InGameOnlyPacketHandler
    {
        private readonly IPacketTranslator<IRefreshReplyData> _refreshReplyTranslator;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IMapChangedNotifier> _mapChangedNotifiers;

        public override PacketFamily Family { get { return PacketFamily.Refresh; } }

        public override PacketAction Action { get { return PacketAction.Reply; } }

        public RefreshMapStateHandler(IPlayerInfoProvider playerInfoProvider,
                                      IPacketTranslator<IRefreshReplyData> refreshReplyTranslator,
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

            var updatedMainCharacter = data.Characters.Single(NameMatches);
            var updatedRenderProperties = _characterRepository.MainCharacter.RenderProperties
                .WithMapX(updatedMainCharacter.RenderProperties.MapX)
                .WithMapY(updatedMainCharacter.RenderProperties.MapY);

            var withoutMainCharacter = data.Characters.Where(x => !NameMatches(x));
            data = data.WithCharacters(withoutMainCharacter);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithRenderProperties(updatedRenderProperties);

            _currentMapStateRepository.Characters = data.Characters.ToList();
            _currentMapStateRepository.NPCs = data.NPCs.ToList();
            _currentMapStateRepository.MapItems = data.Items.ToList();

            foreach (var notifier in _mapChangedNotifiers)
                notifier.NotifyMapChanged(showMapTransition: false, warpAnimation: WarpAnimation.None);

            return true;
        }

        private bool NameMatches(ICharacter x)
        {
            //todo: figure out why ID isn't matching
            return x.Name.ToLower() == _characterRepository.MainCharacter.Name.ToLower();
        }
    }
}
