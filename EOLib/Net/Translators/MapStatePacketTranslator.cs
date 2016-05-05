// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.IO;
using System.Linq;
using EOLib.Domain.BLL;
using EOLib.Domain.Character;
using EOLib.Domain.Map;

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

				var direction = packet.ReadShort();
				packet.ReadChar(); //value is always 6? Unknown use
				var guildTag = packet.ReadString(3);

				var level = packet.ReadChar();
				var gender = packet.ReadChar();
				var hairStyle = packet.ReadChar();
				var hairColor = packet.ReadChar();
				var race = packet.ReadChar();

				var maxHP = packet.ReadShort();
				var hp = packet.ReadShort();
				var MaxTP = packet.ReadShort();
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

				yield return new Character();
			}
		}

		protected IEnumerable<NPC> GetNPCs(IPacket packet)
		{
			return Enumerable.Empty<NPC>();
		}

		protected IEnumerable<MapItem> GetMapItems(IPacket packet)
		{
			return Enumerable.Empty<MapItem>();
		}
	}
}
