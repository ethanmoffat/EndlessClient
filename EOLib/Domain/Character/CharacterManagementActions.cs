using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Linq;
using System.Threading.Tasks;

namespace EOLib.Domain.Character
{
    [AutoMappedType]
    public class CharacterManagementActions : ICharacterManagementActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly ICharacterSelectorRepository _characterSelectorRepository;

        public CharacterManagementActions(IPacketSendService packetSendService,
                                          ICharacterSelectorRepository characterSelectorRepository)
        {
            _packetSendService = packetSendService;
            _characterSelectorRepository = characterSelectorRepository;
        }

        public async Task<int> RequestCharacterCreation()
        {
            var packet = new CharacterRequestClientPacket();
            var response= await _packetSendService.SendEncodedPacketAndWaitAsync(packet);

            if (response is CharacterReplyServerPacket responsePacket)
                return (int)responsePacket.ReplyCode;

            throw new EmptyPacketReceivedException();
        }

        public async Task<CharacterReply> CreateCharacter(ICharacterCreateParameters parameters, int createID)
        {
            var packet = new CharacterCreateClientPacket
            {
                SessionId = createID,
                Gender = (Gender)parameters.Gender,
                HairStyle = parameters.HairStyle,
                HairColor = parameters.HairColor,
                Skin = parameters.Race,
                Name = parameters.Name,
            };

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(packet);
            if (!(response is CharacterReplyServerPacket responsePacket))
                throw new EmptyPacketReceivedException();
            
            if (responsePacket.ReplyCodeData is CharacterReplyServerPacket.ReplyCodeDataOk dataOk && dataOk.Characters.Any())
            {
                _characterSelectorRepository.Characters = dataOk.Characters
                    .Select(Character.FromCharacterSelectionListEntry).ToList();
            }

            return responsePacket.ReplyCode;
        }

        public async Task<int> RequestCharacterDelete()
        {
            var clientPacket = _characterSelectorRepository.CharacterForDelete.Match(
                c => new CharacterTakeClientPacket { CharacterId = c.ID },
                () => null) ?? throw new NoDataSentException();

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(clientPacket);
            if (!(response is CharacterPlayerServerPacket responsePacket))
                throw new EmptyPacketReceivedException();

            return responsePacket.CharacterId;
        }

        public async Task<CharacterReply> DeleteCharacter(int deleteRequestID)
        {
            var clientPacket = _characterSelectorRepository.CharacterForDelete.Match(
                c => new CharacterRemoveClientPacket
                {
                    SessionId = deleteRequestID,
                    CharacterId = c.ID
                },
                () => null) ?? throw new NoDataSentException();

            var response = await _packetSendService.SendEncodedPacketAndWaitAsync(clientPacket);
            if (!(response is CharacterReplyServerPacket responsePacket))
                throw new EmptyPacketReceivedException();

            if (responsePacket.ReplyCodeData is CharacterReplyServerPacket.ReplyCodeDataDeleted dataDeleted && dataDeleted.Characters.Any())
            {
                _characterSelectorRepository.Characters = dataDeleted.Characters
                    .Select(Character.FromCharacterSelectionListEntry).ToList();
            }

            return responsePacket.ReplyCode;
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