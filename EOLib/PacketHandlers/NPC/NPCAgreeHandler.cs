using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;

namespace EOLib.PacketHandlers.NPC
{
    /// <summary>
    /// Sent in response to an NpcMapInfo request
    /// </summary>
    [AutoMappedType]
    public class NPCAgreeHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly INPCFromPacketFactory _npcFromPacketFactory;

        public override PacketFamily Family => PacketFamily.NPC;

        public override PacketAction Action => PacketAction.Agree;

        public NPCAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                               ICurrentMapStateRepository currentMapStateRepository,
                               INPCFromPacketFactory npcFromPacketFactory)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _npcFromPacketFactory = npcFromPacketFactory;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var length = packet.ReadChar();

            for (int i = 0; i < length; i++)
            {
                var npc = _npcFromPacketFactory.CreateNPC(packet);
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
