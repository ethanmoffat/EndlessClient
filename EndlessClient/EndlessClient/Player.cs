using System.Linq;
using EOLib.Net;

namespace EndlessClient
{
	public class Player
	{
		private Character _activeCharacter;
		public string AccountName { get; private set; }
		public int PlayerID { get; private set; }

		public CharRenderData[] CharData { get; private set; }

		public Character ActiveCharacter
		{
			get
			{
				//This (theoretically) prevents a race condition in which a background thread calls "Logout"
				//	and sets _activeCharacter to null. By recreating a dummy character we are able to prevent
				//	the crashes that cause null reference exceptions (again, theoretically)

				if (_activeCharacter == null)
				{
					return new Character();
				}

				return _activeCharacter;
			}
			private set { _activeCharacter = value; }
		}

		public Player()
		{
			Logout();
		}

		public void Logout()
		{
			if (World.Initialized && ActiveCharacter != null)
				Logger.Log("Logging out MainPlayer. Setting ActiveCharacter = null.");
			PlayerID = 0;
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

		public bool SetActiveCharacter(PacketAPI api, int id)
		{
			if (World.Initialized && ActiveCharacter != null)
				Logger.Log("Setting Active Character to new ID {0}", id);
			CharRenderData activeData = CharData.FirstOrDefault(d => d.id == id);
			if (activeData == null)
				return false;
			ActiveCharacter = new Character(api, id, activeData);
			return true;
		}
		
		public void ProcessCharacterData(CharacterRenderData[] dataArray)
		{
			CharData = new CharRenderData[dataArray.Length];
			for (int i = 0; i < CharData.Length; ++i)
			{
				CharData[i] = new CharRenderData
				{
					name = dataArray[i].Name,
					id = dataArray[i].ID,
					level = dataArray[i].Level,
					gender = dataArray[i].Gender,
					hairstyle = dataArray[i].HairStyle,
					haircolor = dataArray[i].HairColor,
					race = dataArray[i].Race,
					admin = (byte)dataArray[i].AdminLevel,
					boots = dataArray[i].Boots,
					armor = dataArray[i].Armor,
					hat = dataArray[i].Hat,
					shield = dataArray[i].Shield,
					weapon = dataArray[i].Weapon
				};
			}
		}
	}
}
