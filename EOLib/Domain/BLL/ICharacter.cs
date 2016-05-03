// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;
using EOLib.IO;

namespace EOLib.Domain.BLL
{
	public interface ICharacter
	{
		int ID { get; }

		string Name { get; }

		string Title { get; }

		//todo: guild stuff should be in a guild object
		string GuildName { get; }

		string GuildRank { get; }

		string GuildTag { get; }

		ClassRecord Class { get; }

		ICharacterRenderProperties RenderProperties { get; }

		ICharacterStats Stats { get; }

		AdminLevel AdminLevel { get; }

		ICharacter WithID(int id);

		ICharacter WithName(string name);

		ICharacter WithTitle(string title);

		ICharacter WithGuildName(string guildName);

		ICharacter WithGuildRank(string guildRank);

		ICharacter WithGuildTag(string guildTag);

		ICharacter WithClass(ClassRecord newClass);

		ICharacter WithRenderProperties(ICharacterRenderProperties renderProperties);

		ICharacter WithStats(ICharacterStats stats);

		ICharacter WithAdminLevel(AdminLevel level);
	}
}
