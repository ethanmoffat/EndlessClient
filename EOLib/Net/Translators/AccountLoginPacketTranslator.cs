// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.Login;

namespace EOLib.Net.Translators
{
	public class AccountLoginPacketTranslator : CharacterDisplayPacketTranslator<IAccountLoginData>
	{
		public override IAccountLoginData TranslatePacket(IPacket packet)
		{
			var reply = (LoginReply) packet.ReadShort();

			var characters = new List<ICharacter>();
			if (reply == LoginReply.Ok)
				characters.AddRange(GetCharacters(packet));

			return new AccountLoginData(reply, characters);
		}
	}
}
