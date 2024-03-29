﻿using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;

namespace EOLib.PacketHandlers.MapInfo
{
    [AutoMappedType]
    public class MapInfoReplyHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICharacterFromPacketFactory _characterFromPacketFactory;
        private readonly INPCFromPacketFactory _npcFromPacketFactory;
        private readonly IEIFFileProvider _eifFileProvider;

        public override PacketFamily Family => PacketFamily.MapInfo;

        public override PacketAction Action => PacketAction.Reply;

        public MapInfoReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   ICharacterFromPacketFactory characterFromPacketFactory,
                                   INPCFromPacketFactory npcFromPacketFactory,
                                   IEIFFileProvider eifFileProvider)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _characterFromPacketFactory = characterFromPacketFactory;
            _npcFromPacketFactory = npcFromPacketFactory;
            _eifFileProvider = eifFileProvider;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var numberOfCharacters = packet.ReadChar();

            if (packet.PeekByte() == byte.MaxValue)
            {
                packet.ReadByte();
                for (var i = 0; i < numberOfCharacters; i++)
                {
                    var character = _characterFromPacketFactory.CreateCharacter(packet);
                    if (_currentMapStateRepository.Characters.TryGetValue(character.ID, out var existingCharacter))
                    {
                        character = existingCharacter.WithAppliedData(character);
                    }

                    _currentMapStateRepository.Characters.Update(existingCharacter, character);
                    if (packet.ReadByte() != byte.MaxValue)
                        throw new MalformedPacketException("Missing 255 byte after character data", packet);
                }
            }

            while (packet.ReadPosition < packet.Length)
            {
                var npc = _npcFromPacketFactory.CreateNPC(packet);
                if (_currentMapStateRepository.NPCs.ContainsKey(npc.Index))
                    _currentMapStateRepository.NPCs.Update(_currentMapStateRepository.NPCs[npc.Index], npc);
                else
                    _currentMapStateRepository.NPCs.Add(npc);
            }

            return true;
        }
    }
}
