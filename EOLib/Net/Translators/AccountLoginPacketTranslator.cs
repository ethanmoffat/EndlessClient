// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.IO;
using EOLib.Domain.BLL;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.API;

namespace EOLib.Net.Translators
{
	public class AccountLoginPacketTranslator : IPacketTranslator<IAccountLoginData>
	{
		public IAccountLoginData TranslatePacket(IPacket packet)
		{
			var reply = (LoginReply)packet.ReadShort();

			var characters = new List<ICharacter>();
			if (reply == LoginReply.Ok)
			{

				var numberOfCharacters = (int) packet.ReadChar();
				packet.Seek(2, SeekOrigin.Current);

				for (int i = 0; i < numberOfCharacters; ++i)
				{
					characters.Add(GetNextCharacter(packet));
					if(packet.ReadByte() != 255)
						throw new MalformedPacketException("Login packet missing character separator byte", packet);
				}
			}

			return new AccountLoginData(reply, characters);
		}

		private ICharacter GetNextCharacter(IPacket packet)
		{
			ICharacter character = new Character()
				.WithName(packet.ReadBreakString())
				.WithID(packet.ReadInt());

			var stats = new CharacterStats()
				.WithNewStat(CharacterStat.Level, packet.ReadChar());

			var renderProperties = new CharacterRenderProperties()
				.WithGender(packet.ReadChar())
				.WithHairStyle(packet.ReadChar())
				.WithHairColor(packet.ReadChar())
				.WithRace(packet.ReadChar());

			character = character.WithAdminLevel((AdminLevel) packet.ReadChar());

			renderProperties = renderProperties
				.WithBootsGraphic(packet.ReadShort())
				.WithArmorGraphic(packet.ReadShort())
				.WithHatGraphic(packet.ReadShort())
				.WithShieldGraphic(packet.ReadShort())
				.WithWeaponGraphic(packet.ReadShort());

			return character
				.WithRenderProperties(renderProperties)
				.WithStats(stats);
		}
	}
}
