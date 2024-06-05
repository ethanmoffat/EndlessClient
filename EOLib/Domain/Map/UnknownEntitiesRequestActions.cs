using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using System.Linq;

namespace EOLib.Domain.Map
{
    [AutoMappedType]
    public class UnknownEntitiesRequestActions : IUnknownEntitiesRequestActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public UnknownEntitiesRequestActions(IPacketSendService packetSendService,
                                             ICurrentMapStateRepository currentMapStateRepository)
        {
            _packetSendService = packetSendService;
            _currentMapStateRepository = currentMapStateRepository;
        }

        public void RequestUnknownPlayers()
        {
            var packet = new PlayerRangeRequestClientPacket
            {
                PlayerIds = _currentMapStateRepository.UnknownPlayerIDs.ToList()
            };
            _packetSendService.SendPacket(packet);
            _currentMapStateRepository.UnknownPlayerIDs.Clear();
        }

        public void RequestUnknownNPCs()
        {
            var packet = new NpcRangeRequestClientPacket
            {
                NpcIndexes = _currentMapStateRepository.UnknownNPCIndexes.ToList()
            };
            _packetSendService.SendPacket(packet);
            _currentMapStateRepository.UnknownNPCIndexes.Clear();
        }

        public void RequestAll()
        {
            var packet = new RangeRequestClientPacket
            {
                PlayerIds = _currentMapStateRepository.UnknownPlayerIDs.ToList(),
                NpcIndexes = _currentMapStateRepository.UnknownNPCIndexes.ToList(),
            };
            _packetSendService.SendPacket(packet);
            _currentMapStateRepository.UnknownPlayerIDs.Clear();
            _currentMapStateRepository.UnknownNPCIndexes.Clear();
        }
    }

    public interface IUnknownEntitiesRequestActions
    {
        void RequestUnknownPlayers();

        void RequestUnknownNPCs();

        void RequestAll();
    }
}
