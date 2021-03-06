﻿using AutomaticTypeMapper;
using EOLib.Domain.Map;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class WarpAgreePacketTranslator : MapStatePacketTranslator<IWarpAgreePacketData>
    {
        public WarpAgreePacketTranslator(ICharacterFromPacketFactory characterFromPacketFactory)
            : base(characterFromPacketFactory) { }

        public override IWarpAgreePacketData TranslatePacket(IPacket packet)
        {
            IWarpAgreePacketData retData = new WarpAgreePacketData();

            if (packet.ReadChar() != 2)
                throw new MalformedPacketException("Missing initial marker value of 2", packet);

            var newMapID = packet.ReadShort();
            var warpAnim = (WarpAnimation)packet.ReadChar();

            var characters = GetCharacters(packet);
            var npcs = GetNPCs(packet);
            var items = GetMapItems(packet);

            return retData
                .WithMapID(newMapID)
                .WithWarpAnimation(warpAnim)
                .WithCharacters(characters)
                .WithNPCs(npcs)
                .WithItems(items);
        }
    }
}
