using AutomaticTypeMapper;
using EOLib.Domain.Map;
using System.Linq;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class RefreshReplyPacketTranslator : MapStatePacketTranslator<RefreshReplyData>
    {
        public RefreshReplyPacketTranslator(ICharacterFromPacketFactory characterFromPacketFactory)
            : base(characterFromPacketFactory) { }

        public override RefreshReplyData TranslatePacket(IPacket packet)
        {
            var characters = GetCharacters(packet);
            var npcs = GetNPCs(packet);
            var items = GetMapItems(packet);

            return new RefreshReplyData.Builder
            {
                Characters = characters.ToList(),
                NPCs = npcs.ToList(),
                Items = items.ToList(),
            }.ToImmutable();
        }
    }
}
