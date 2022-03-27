using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.IO.Extensions;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class MapInfoHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICharacterFromPacketFactory _characterFromPacketFactory;
        private readonly IEIFFileProvider _eifFileProvider;

        public override PacketFamily Family => PacketFamily.MapInfo;

        public override PacketAction Action => PacketAction.Reply;

        public MapInfoHandler(IPlayerInfoProvider playerInfoProvider,
                              ICurrentMapStateRepository currentMapStateRepository,
                              ICharacterFromPacketFactory characterFromPacketFactory,
                              IEIFFileProvider eifFileProvider
                              )
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _characterFromPacketFactory = characterFromPacketFactory;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var num_entities = packet.ReadChar();

            if (packet.PeekByte() == 0xFF)
            {
                for (var i = 0; i < num_entities; i++)
                {
                    var character = _characterFromPacketFactory.CreateCharacter(packet);
                    if (_currentMapStateRepository.Characters.ContainsKey(character.ID))
                    {
                        var existingCharacter = _currentMapStateRepository.Characters[character.ID];
                        var isRangedWeapon = _eifFileProvider.EIFFile.IsRangedWeapon(character.RenderProperties.WeaponGraphic);
                        character = existingCharacter.WithAppliedData(character, isRangedWeapon);
                    }
                    _currentMapStateRepository.Characters[character.ID] = character;
                }
            }

            while (packet.ReadPosition < packet.Length)
            {
                var index = packet.ReadChar();
                var id = packet.ReadShort();
                var x = packet.ReadChar();
                var y = packet.ReadChar();
                var direction = (EODirection)packet.ReadChar();

                INPC npc = new NPC(id, index);
                npc = npc.WithX(x).WithY(y).WithDirection(direction).WithFrame(NPCFrame.Standing);

                _currentMapStateRepository.NPCs.RemoveWhere(n => n.Index == index);
                _currentMapStateRepository.NPCs.Add(npc);
            }

            return true;
        }
    }
}
