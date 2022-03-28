using AutomaticTypeMapper;
using EOLib.Domain.NPC;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class NPCFromPacketFactory : INPCFromPacketFactory
    {
        public INPC CreateNPC(IPacket packet)
        {
            var index = packet.ReadChar();
            var id = packet.ReadShort();
            var x = packet.ReadChar();
            var y = packet.ReadChar();
            var direction = (EODirection)packet.ReadChar();

            INPC npc = new NPC(id, index);
            npc = npc.WithX(x).WithY(y).WithDirection(direction).WithFrame(NPCFrame.Standing);
            return npc;
        }
    }

    public interface INPCFromPacketFactory
    {
        INPC CreateNPC(IPacket packet);
    }
}
