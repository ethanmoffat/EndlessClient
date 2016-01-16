// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO;

namespace EndlessClient
{
	public interface ISpellIcon
	{
		int Slot { get; set; }

		bool Selected { get; set; }

		bool IsDragging { get; }

		short Level { get; set; }

		SpellRecord SpellData { get; }

		void SetDisplaySlot(int displaySlot);

		//defined in XNAControl base class
		bool MouseOver { get; }

		bool Visible { get; set; }
	}
}