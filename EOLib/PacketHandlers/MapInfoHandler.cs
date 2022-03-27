using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
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
        private readonly INPCFromPacketFactory _npcFromPacketFactory;
        private readonly IEIFFileProvider _eifFileProvider;

        public override PacketFamily Family => PacketFamily.MapInfo;

        public override PacketAction Action => PacketAction.Reply;

        public MapInfoHandler(IPlayerInfoProvider playerInfoProvider,
                              ICurrentMapStateRepository currentMapStateRepository,
                              ICharacterFromPacketFactory characterFromPacketFactory,
                              INPCFromPacketFactory npcFromPacketFactory
                              )
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _characterFromPacketFactory = characterFromPacketFactory;
            _npcFromPacketFactory = npcFromPacketFactory;
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
                var npc = _npcFromPacketFactory.CreateNPC(packet);
                _currentMapStateRepository.NPCs.RemoveWhere(n => n.Index == npc.Index);
                _currentMapStateRepository.NPCs.Add(npc);
            }

            return true;
        }
    }
}
