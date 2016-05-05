// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;

namespace EOLib.Net.Translators
{
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
