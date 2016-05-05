// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.IO;
using System.Linq;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;

namespace EOLib.Net.Translators
{
	public abstract class MapStatePacketTranslator<T> : IPacketTranslator<T>
		where T : ITranslatedData
	{
		public abstract T TranslatePacket(IPacket packet);

		protected IEnumerable<ICharacter> GetCharacters(IPacket packet)
		{
			var numCharacters = packet.ReadChar();
			for (int i = 0; i < numCharacters; ++i)
			{
				var name = packet.ReadBreakString();
				name = char.ToUpper(name[0]) + name.Substring(1);

				var id = packet.ReadShort();
				var mapID = packet.ReadShort();
				var xLoc = packet.ReadShort();
				var yLoc = packet.ReadShort();

				var direction = (EODirection)packet.ReadChar();
				packet.ReadChar(); //value is always 6? Unknown use
				var guildTag = packet.ReadString(3);

				var level = packet.ReadChar();
				var gender = packet.ReadChar();
				var hairStyle = packet.ReadChar();
				var hairColor = packet.ReadChar();
				var race = packet.ReadChar();

				var maxHP = packet.ReadShort();
				var hp = packet.ReadShort();
				var maxTP = packet.ReadShort();
				var tp = packet.ReadShort();

				var boots = packet.ReadShort();
				packet.Seek(6, SeekOrigin.Current); //0s
				var armor = packet.ReadShort();
				packet.Seek(2, SeekOrigin.Current); //0
				var hat = packet.ReadShort();
				var shield = packet.ReadShort();
				var weapon = packet.ReadShort();

				var sitState = (SitState) packet.ReadChar();
				var hidden = packet.ReadChar() != 0;

				var stats = new CharacterStats()
					.WithNewStat(CharacterStat.Level, level)
					.WithNewStat(CharacterStat.HP, hp)
					.WithNewStat(CharacterStat.MaxHP, maxHP)
					.WithNewStat(CharacterStat.TP, tp)
					.WithNewStat(CharacterStat.MaxTP, maxTP);

				var renderProps = new CharacterRenderProperties()
					.WithDirection(direction)
					.WithGender(gender)
					.WithHairStyle(hairStyle)
					.WithHairColor(hairColor)
					.WithRace(race)
					.WithBootsGraphic(boots)
					.WithArmorGraphic(armor)
					.WithHatGraphic(hat)
					.WithShieldGraphic(shield)
					.WithWeaponGraphic(weapon)
					.WithSitState(sitState)
					.WithIsHidden(hidden);

				yield return new Character()
					.WithName(name)
					.WithID(id)
					.WithMapID(mapID)
					.WithMapX(xLoc)
					.WithMapY(yLoc)
					.WithGuildTag(guildTag)
					.WithStats(stats)
					.WithRenderProperties(renderProps);
			}
		}

		protected IEnumerable<INPC> GetNPCs(IPacket packet)
		{
			return Enumerable.Empty<INPC>();
		}

		protected IEnumerable<OldMapItem> GetMapItems(IPacket packet)
		{
			return Enumerable.Empty<MapItem>();
		}
	}
}
