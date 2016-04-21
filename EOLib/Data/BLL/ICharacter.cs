// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.API;

namespace EOLib.Data.BLL
{
	public interface ICharacter
	{
		string Name { get; }

		int ID { get; }

		ICharacterRenderProperties RenderProperties { get; }

		ICharacterStats Stats { get; }

		AdminLevel AdminLevel { get; }

		ICharacter WithName(string name);

		ICharacter WithID(int id);

		ICharacter WithRenderProperties(ICharacterRenderProperties renderProperties);

		ICharacter WithStats(ICharacterStats stats);

		ICharacter WithAdminLevel(AdminLevel level);
	}
}
