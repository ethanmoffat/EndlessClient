// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.IO.Map
{
	public struct MapItem
	{
		public short uid;
		public short id;
		public byte x;
		public byte y;
		public int amount;
		public DateTime time;
		public bool npcDrop;
		public int playerID;
	}
}