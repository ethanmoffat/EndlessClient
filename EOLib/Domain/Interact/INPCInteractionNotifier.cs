using AutomaticTypeMapper;
using EOLib.IO;

namespace EOLib.Domain.Interact
{
    public interface INPCInteractionNotifier
    {
        void NotifyInteractionFromNPC(NPCType npcType);
    }

    [AutoMappedType]
    public class NoOpNPCInteractionNotifier : INPCInteractionNotifier
    {
        public void NotifyInteractionFromNPC(NPCType npcType) { }
    }
}
