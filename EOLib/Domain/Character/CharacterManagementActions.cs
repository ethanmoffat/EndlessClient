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

        public async Task<int> RequestCharacterCreation()
        {
            var packet = new PacketBuilder(PacketFamily.Character, PacketAction.Request)
                .AddBreakString("NEW")
                .Build();
            var responsePacket = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            return responsePacket.ReadShort();
        }

        public async Task<CharacterReply> CreateCharacter(ICharacterCreateParameters parameters, int createID)
        {
            var packet = new PacketBuilder(PacketFamily.Character, PacketAction.Create)
                .AddShort(createID)
                .AddShort(parameters.Gender)
                .AddShort(parameters.HairStyle)
                .AddShort(parameters.HairColor)
                .AddShort(parameters.Race)
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
            var packet = _characterSelectorRepository.CharacterForDelete.Match(
                some: c => new PacketBuilder(PacketFamily.Character, PacketAction.Take).AddInt(c.ID).Build(),
                none: () => new EmptyPacket());
            
            var responsePacket = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            var deleteRequestId = responsePacket.ReadShort();

            return deleteRequestId;
        }

        public async Task<CharacterReply> DeleteCharacter(int deleteRequestID)
        {
            var packet = _characterSelectorRepository.CharacterForDelete.Match(
                some: c => new PacketBuilder(PacketFamily.Character, PacketAction.Remove).AddShort(deleteRequestID).AddInt(c.ID).Build(),
                none: () => new EmptyPacket());

            var responsePacket = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);

            var translatedData = _characterCreatePacketTranslator.TranslatePacket(responsePacket);
            _characterSelectorRepository.Characters = translatedData.Characters;
            return translatedData.Response;
        }
    }

    public interface ICharacterManagementActions
    {
        Task<int> RequestCharacterCreation();

        Task<CharacterReply> CreateCharacter(ICharacterCreateParameters parameters, int createID);

        Task<int> RequestCharacterDelete();

        Task<CharacterReply> DeleteCharacter(int deleteRequestID);
    }
}