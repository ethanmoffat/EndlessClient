using System.Linq;
using EOLib;

namespace EndlessClient
{
	public class Player
	{
		public string AccountName { get; private set; }
		public int PlayerID { get; private set; }
		//not sure what GamePlayerID is so commenting it out...
		//public int GamePlayerID { get; private set; }

		public CharRenderData[] CharData { get; private set; }
		public Character ActiveCharacter { get; private set; }
		
		public Player()
		{
			Logout();
		}

		public void Logout()
		{
			PlayerID = 0;
			//GamePlayerID = 0;
			CharData = null;
			ActiveCharacter = null;
		}

		public void SetAccountName(string acc)
		{
			AccountName = acc;
		}

		public void SetPlayerID(int newId)
		{
			PlayerID = newId;
		}

		//public void SetGamePlayerID(int newId)
		//{
		//	GamePlayerID = newId;
		//}

		public bool SetActiveCharacter(int id)
		{
			CharRenderData activeData = CharData.FirstOrDefault(d => d.id == id);
			if (activeData == null)
				return false;
			ActiveCharacter = new Character(id, activeData);
			return true;
		}
		
		public void ProcessCharacterData(Packet pkt)
		{
			byte numCharacters = pkt.GetChar();
			pkt.GetByte();
			pkt.GetByte();

			CharRenderData[] characters = new CharRenderData[numCharacters];

			for (int i = 0; i < numCharacters; ++i)
			{
				CharRenderData nextData = new CharRenderData
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

			CharData = characters;
		}
	}
}
