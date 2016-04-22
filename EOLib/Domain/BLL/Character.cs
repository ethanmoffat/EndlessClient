// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.API;

namespace EOLib.Domain.BLL
{
	public class Character : ICharacter
	{
		public string Name { get; private set; }

		public int ID { get; private set; }

		public AdminLevel AdminLevel { get; private set; }

		public ICharacterRenderProperties RenderProperties { get; private set; }

		public ICharacterStats Stats { get; private set; }

		public ICharacter WithName(string name)
		{
			var character = MakeCopy(this);
			character.Name = name;
			return character;
		}

		public ICharacter WithID(int id)
		{
			var character = MakeCopy(this);
			character.ID = id;
			return character;
		}

		public ICharacter WithAdminLevel(AdminLevel level)
		{
			var character = MakeCopy(this);
			character.AdminLevel = level;
			return character;
		}

		public ICharacter WithRenderProperties(ICharacterRenderProperties renderProperties)
		{
			var character = MakeCopy(this);
			character.RenderProperties = renderProperties;
			return character;
		}

		public ICharacter WithStats(ICharacterStats stats)
		{
			var character = MakeCopy(this);
			character.Stats = stats;
			return character;
		}

		private static Character MakeCopy(ICharacter source)
		{
			return new Character
			{
				Name = source.Name,
				ID = source.ID,
				AdminLevel = source.AdminLevel,
				RenderProperties = source.RenderProperties,
				Stats = source.Stats
			};
		}
	}
}