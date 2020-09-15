using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Protocol;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class AccountLoginPacketTranslator : CharacterDisplayPacketTranslator<IAccountLoginData>
    {
        public override IAccountLoginData TranslatePacket(IPacket packet)
        {
            LoginReply reply;
            var characters = new List<ICharacter>();

            if (packet.Family == PacketFamily.Login && packet.Action == PacketAction.Reply)
            {
                reply = (LoginReply)packet.ReadShort();

                if (reply == LoginReply.Ok)
                    characters.AddRange(GetCharacters(packet));
            }
            else if (packet.Family == PacketFamily.Init && packet.Action == PacketAction.Init)
            {
                var initReply = (InitReply)packet.ReadByte();
                var banType = (BanType)packet.ReadByte();

                if (initReply != InitReply.BannedFromServer && banType != BanType.PermanentBan)
                    reply = LoginReply.THIS_IS_WRONG;
                else
                    reply = LoginReply.AccountBanned;
            }
            else
            {
                reply = LoginReply.THIS_IS_WRONG;
            }

            return new AccountLoginData(reply, characters);
        }
    }
}
