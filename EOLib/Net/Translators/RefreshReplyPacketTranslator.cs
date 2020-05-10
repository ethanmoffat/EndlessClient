using AutomaticTypeMapper;
using EOLib.Domain.Map;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class RefreshReplyPacketTranslator : MapStatePacketTranslator<IRefreshReplyData>
    {
        public RefreshReplyPacketTranslator(ICharacterFromPacketFactory characterFromPacketFactory)
            : base(characterFromPacketFactory) { }

        public override IRefreshReplyData TranslatePacket(IPacket packet)
        {
            var characters = GetCharacters(packet);
            var npcs = GetNPCs(packet);
            var items = GetMapItems(packet);

            IRefreshReplyData data = new RefreshReplyData();

            return data.WithCharacters(characters)
                .WithNPCs(npcs)
                .WithItems(items);
        }
    }
}
