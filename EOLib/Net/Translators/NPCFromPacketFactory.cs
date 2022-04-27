using AutomaticTypeMapper;
using EOLib.Domain.NPC;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class NPCFromPacketFactory : INPCFromPacketFactory
    {
        public NPC CreateNPC(IPacket packet)
        {
            var index = packet.ReadChar();
            var id = packet.ReadShort();
            var x = packet.ReadChar();
            var y = packet.ReadChar();
            var direction = (EODirection)packet.ReadChar();

            var npc = new NPC.Builder()
            {
                ID = id,
                Index = index,
                X = x,
                Y = y,
                Direction = direction,
                Frame = NPCFrame.Standing,
            };
            return npc.ToImmutable();
        }
    }

    public interface INPCFromPacketFactory
    {
        NPC CreateNPC(IPacket packet);
    }
}
