using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Linq;

namespace EOLib.PacketHandlers.MapInfo
{
    [AutoMappedType]
    public class MapInfoReplyHandler : InGameOnlyPacketHandler<RangeReplyServerPacket>
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Range;

        public override PacketAction Action => PacketAction.Reply;

        public MapInfoReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(RangeReplyServerPacket packet)
        {
            foreach (var character in packet.Nearby.Characters.Where(x => x.ByteSize >= 42).Select(Character.FromNearby))
            {
                if (_currentMapStateRepository.Characters.ContainsKey(character.ID))
                {
                    _currentMapStateRepository.Characters.Update(
                        _currentMapStateRepository.Characters[character.ID],
                        _currentMapStateRepository.Characters[character.ID].WithAppliedData(character));
                }
                else
                    _currentMapStateRepository.Characters.Add(character);
            }

            foreach (var npc in packet.Nearby.Npcs.Select(Domain.NPC.NPC.FromNearby))
            {
                if (_currentMapStateRepository.NPCs.ContainsKey(npc.Index))
                    _currentMapStateRepository.NPCs.Update(_currentMapStateRepository.NPCs[npc.Index], npc);
                else
                    _currentMapStateRepository.NPCs.Add(npc);
            }

            return true;
        }
    }
}
