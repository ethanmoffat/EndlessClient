// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class NPCEnterMapHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Appear;

        public override PacketAction Action => PacketAction.Reply;

        public NPCEnterMapHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            if (packet.Length - packet.ReadPosition != 8)
                throw new MalformedPacketException("Invalid packet length for new NPC in map.", packet);
            if (!packet.ReadBytes(2).SequenceEqual(new byte[] {1, 255}))
                throw new MalformedPacketException("Invalid header data for new NPC in map. Expected byte values 0x01, 0xff.", packet);

            var index = packet.ReadChar();
            var id = packet.ReadShort();
            var x = packet.ReadChar();
            var y = packet.ReadChar();
            var direction = (EODirection) packet.ReadChar();

            INPC npc = new NPC(id, index);
            npc = npc.WithX(x).WithY(y).WithDirection(direction).WithFrame(NPCFrame.Standing);

            _currentMapStateRepository.NPCs.RemoveAll(n => n.Index == index);
            _currentMapStateRepository.NPCs.Add(npc);

            return true;
        }
    }
}
