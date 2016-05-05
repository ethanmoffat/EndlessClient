// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Character
{
	public class InventorySpell : IInventorySpell
	{
		public short ID { get; private set; }

		public short Level { get; private set; }

		public InventorySpell(short id, short level)
		{
			ID = id;
			Level = level;
		}

		public IInventorySpell WithLevel(short newLevel)
		{
			return new InventorySpell(ID, newLevel);
		}
	}
}