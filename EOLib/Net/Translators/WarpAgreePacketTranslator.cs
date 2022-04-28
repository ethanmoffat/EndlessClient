using AutomaticTypeMapper;
using EOLib.Domain.Map;
using System.Linq;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class WarpAgreePacketTranslator : MapStatePacketTranslator<WarpAgreePacketData>
    {
        public WarpAgreePacketTranslator(ICharacterFromPacketFactory characterFromPacketFactory)
            : base(characterFromPacketFactory) { }

        public override WarpAgreePacketData TranslatePacket(IPacket packet)
        {
            if (packet.ReadChar() != 2)
                throw new MalformedPacketException("Missing initial marker value of 2", packet);

            var newMapID = packet.ReadShort();
            var warpAnim = (WarpAnimation)packet.ReadChar();

            var characters = GetCharacters(packet);
            var npcs = GetNPCs(packet);
            var items = GetMapItems(packet);

            return new WarpAgreePacketData.Builder
            { 
                MapID = newMapID,
                WarpAnimation = warpAnim,
                Characters = characters.ToList(),
                NPCs = npcs.ToList(),
                Items = items.ToList(),
            }.ToImmutable();
        }
    }
}
