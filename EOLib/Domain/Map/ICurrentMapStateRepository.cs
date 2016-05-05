// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.NPC;

namespace EOLib.Domain.Map
{
	public interface ICurrentMapStateRepository
	{
		short CurrentMapID { get; set; }

		List<ICharacter> Characters { get; set; }

		List<INPC> NPCs { get; set; }

		List<IMapItem> MapItems { get; set; }
	}

	public interface ICurrentMapProvider
	{
		short CurrentMapID { get; }

		IReadOnlyList<ICharacter> Characters { get; }

		IReadOnlyList<INPC> NPCs { get; }

		IReadOnlyList<IMapItem> MapItems { get; }
	}

	public class CurrentMapStateRepository : ICurrentMapStateRepository, ICurrentMapProvider
	{
		public short CurrentMapID { get; set; }

		public List<ICharacter> Characters { get; set; }

		public List<INPC> NPCs { get; set; }

		public List<IMapItem> MapItems { get; set; }

		IReadOnlyList<ICharacter> ICurrentMapProvider.Characters { get { return Characters; } }

		IReadOnlyList<INPC> ICurrentMapProvider.NPCs { get { return NPCs; } }

		IReadOnlyList<IMapItem> ICurrentMapProvider.MapItems { get { return MapItems; } }

		public CurrentMapStateRepository()
		{
			Characters = new List<ICharacter>();
			NPCs = new List<INPC>();
			MapItems = new List<IMapItem>();
		}
	}
}
