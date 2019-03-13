// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.Translators;

namespace EOLib.Domain.Character
{
    [AutoMappedType]
    public class CharacterManagementActions : ICharacterManagementActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IPacketTranslator<ICharacterCreateData> _characterCreatePacketTranslator;
        private readonly ICharacterSelectorRepository _characterSelectorRepository;

        public CharacterManagementActions(IPacketSendService packetSendService,
                                          IPacketTranslator<ICharacterCreateData> characterCreatePacketTranslator,
                                          ICharacterSelectorRepository characterSelectorRepository)
        {
            _packetSendService = packetSendService;
            _characterCreatePacketTranslator = characterCreatePacketTranslator;
            _characterSelectorRepository = characterSelectorRepository;
        }

        public async Task<short> RequestCharacterCreation()
        {
            var packet = new PacketBuilder(PacketFamily.Character, PacketAction.Request).Build();
            var responsePacket = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            return responsePacket.ReadShort();
        }

        public async Task<CharacterReply> CreateCharacter(ICharacterCreateParameters parameters)
        {
            var packet = new PacketBuilder(PacketFamily.Character, PacketAction.Create)
                .AddShort(255)
                .AddShort((short)parameters.Gender)
                .AddShort((short)parameters.HairStyle)
                .AddShort((short)parameters.HairColor)
                .AddShort((short)parameters.Race)
                .AddByte(255)
                .AddBreakString(parameters.Name)
                .Build();
            var responsePacket = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            
            var translatedData = _characterCreatePacketTranslator.TranslatePacket(responsePacket);
            if (translatedData.Characters.Any())
                _characterSelectorRepository.Characters = translatedData.Characters;
            return translatedData.Response;
        }

        public async Task<int> RequestCharacterDelete()
        {
            var packet = new PacketBuilder(PacketFamily.Character, PacketAction.Take)
                .AddInt(_characterSelectorRepository.CharacterForDelete.ID)
                .Build();
            
            var responsePacket = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            responsePacket.Seek(2, SeekOrigin.Current);
            return responsePacket.ReadInt();
        }

        public async Task<CharacterReply> DeleteCharacter()
        {
            var packet = new PacketBuilder(PacketFamily.Character, PacketAction.Remove)
                .AddShort(255)
                .AddInt(_characterSelectorRepository.CharacterForDelete.ID)
                .Build();
            var responsePacket = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);

            var translatedData = _characterCreatePacketTranslator.TranslatePacket(responsePacket);
            _characterSelectorRepository.Characters = translatedData.Characters;
            return translatedData.Response;
        }
    }

    public interface ICharacterManagementActions
    {
        Task<short> RequestCharacterCreation();

        Task<CharacterReply> CreateCharacter(ICharacterCreateParameters parameters);

        Task<int> RequestCharacterDelete();

        Task<CharacterReply> DeleteCharacter();
    }
}