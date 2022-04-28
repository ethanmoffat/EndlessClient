using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.IO.Repositories;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class CharacterReplyPacketTranslator : CharacterDisplayPacketTranslator<ICharacterCreateData>
    {
        public CharacterReplyPacketTranslator(IEIFFileProvider eifFileProvider)
            : base(eifFileProvider) { }

        public override ICharacterCreateData TranslatePacket(IPacket packet)
        {
            var reply = (CharacterReply) packet.ReadShort();

            var characters = new List<Character>();
            if (reply == CharacterReply.Ok || reply == CharacterReply.Deleted)
                characters.AddRange(GetCharacters(packet));

            return new CharacterCreateData(reply, characters);
        }
    }
}
