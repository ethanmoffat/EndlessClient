using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EOLib;

namespace EndlessClient
{
	public class Player
	{
		public int PlayerID { get; private set; }
		public int GamePlayerID { get; private set; }

		public CharacterData[] CharData { get; private set; }

		public Player()
		{
			Logout();
		}

		public void Logout()
		{
			PlayerID = 0;
			GamePlayerID = 0;
			CharData = null;
		}

		public void SetPlayerID(int newId)
		{
			PlayerID = newId;
		}

		public void SetGamePlayerID(int newId)
		{
			GamePlayerID = newId;
		}

		public void SetCharacterData(CharacterData[] data)
		{
			CharData = data;
		}

		public void ProcessCharacterData(Packet pkt)
		{
			byte numCharacters = pkt.GetChar();
			pkt.GetByte();
			pkt.GetByte();

			CharacterData[] characters = new CharacterData[numCharacters];

			for (int i = 0; i < numCharacters; ++i)
			{
				CharacterData nextData = new CharacterData()
				{
					name = pkt.GetBreakString(),
					id = pkt.GetInt(),
					level = pkt.GetChar(),
					gender = pkt.GetChar(),
					hairstyle = pkt.GetChar(),
					haircolor = pkt.GetChar(),
					race = pkt.GetChar(),
					admin = pkt.GetChar(),
					boots = pkt.GetShort(),
					armor = pkt.GetShort(),
					hat = pkt.GetShort(),
					shield = pkt.GetShort(),
					weapon = pkt.GetShort()
				};
				characters[i] = nextData;
				if (255 != pkt.GetByte())
					break;
			}

			SetCharacterData(characters);
		}
	}
}
