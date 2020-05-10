using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class PlayerEnterMapHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _mapStateRepository;
        private readonly ICharacterFromPacketFactory _characterFromPacketFactory;

        public override PacketFamily Family => PacketFamily.Players;

        public override PacketAction Action => PacketAction.Agree;

        public PlayerEnterMapHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICurrentMapStateRepository mapStateRepository,
                                     ICharacterFromPacketFactory characterFromPacketFactory)
            : base(playerInfoProvider)
        {
            _mapStateRepository = mapStateRepository;
            _characterFromPacketFactory = characterFromPacketFactory;
        }

        public override bool HandlePacket(IPacket packet)
        {
            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Missing 255 header byte for player enter map handler", packet);

            var character = _characterFromPacketFactory.CreateCharacter(packet);

            var anim = WarpAnimation.None;
            if (packet.PeekByte() != 255) //next byte was the warp animation: sent on Map::Enter in eoserv
            {
                //todo: need to signal client that animation should be performed
                anim = (WarpAnimation) packet.ReadChar();
                if (packet.ReadByte() != 255) //the 255 still needs to be read...
                    throw new MalformedPacketException("Missing 255 byte after the warp animation for player enter map handler", packet);
            }
            else //next byte was a 255. Read it and proceed normally.
            {
                packet.ReadByte();
            }

            //0 for NPC, 1 for player. In eoserv it is never 0.
            if (packet.ReadChar() != 1)
                throw new MalformedPacketException(
                    "Missing '1' char after warp animation for player enter map handler. Are you using a non-standard version of EOSERV?",
                    packet);

            var existingCharacter = _mapStateRepository.Characters.SingleOrDefault(x => x.ID == character.ID);
            if (existingCharacter != null)
            {
                character = existingCharacter.WithAppliedData(character);
                _mapStateRepository.Characters.Remove(existingCharacter);
            }
            _mapStateRepository.Characters.Add(character);

            return true;
        }
    }
}
