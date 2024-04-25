using AutomaticTypeMapper;
using EOLib.Domain.Map;
using System.Linq;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class PlayersAgreePacketTranslator : MapStatePacketTranslator<PlayersAgreeData>
    {
        public PlayersAgreePacketTranslator(ICharacterFromPacketFactory characterFromPacketFactory)
            : base(characterFromPacketFactory) { }

        public override PlayersAgreeData TranslatePacket(IPacket packet)
        {
            var characters = GetCharacters(packet);
            var npcs = GetNPCs(packet);
            var items = GetMapItems(packet);

            return new PlayersAgreeData.Builder
            {
                Characters = characters.ToList(),
                NPCs = npcs.ToList(),
                Items = items.ToList(),
            }.ToImmutable();
        }
    }
}
