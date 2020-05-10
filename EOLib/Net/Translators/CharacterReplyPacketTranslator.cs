using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class CharacterReplyPacketTranslator : CharacterDisplayPacketTranslator<ICharacterCreateData>
    {
        public override ICharacterCreateData TranslatePacket(IPacket packet)
        {
            var reply = (CharacterReply) packet.ReadShort();

            var characters = new List<ICharacter>();
            if (reply == CharacterReply.Ok || reply == CharacterReply.Deleted)
                characters.AddRange(GetCharacters(packet));

            return new CharacterCreateData(reply, characters);
        }
    }
}
