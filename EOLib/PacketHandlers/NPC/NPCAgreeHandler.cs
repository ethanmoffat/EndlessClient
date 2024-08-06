using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Sent in response to an NpcMapInfo request
    /// </summary>
    [AutoMappedType]
    public class NPCAgreeHandler : InGameOnlyPacketHandler<NpcAgreeServerPacket>
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Npc;

        public override PacketAction Action => PacketAction.Agree;

        public NPCAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                               ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(NpcAgreeServerPacket packet)
        {
            foreach (var npc in packet.Npcs.Select(Domain.NPC.NPC.FromNearby))
            {
                if (_currentMapStateRepository.NPCs.TryGetValue(npc.Index, out var oldNpc))
                {
                    _currentMapStateRepository.NPCs.Update(oldNpc, npc);
                }
                else
                {
                    _currentMapStateRepository.NPCs.Add(npc);
                }
            }

            return true;
        }
    }
}
