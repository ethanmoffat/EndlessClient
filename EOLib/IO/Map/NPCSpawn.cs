// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
	public class NPCSpawn
	{
		public byte X { get; set; }
		public byte Y { get; set; }
		public short NpcID { get; set; }
		public byte NpcIndex { get; set; }
		public byte SpawnType { get; set; }
		public short RespawnTime { get; set; }
		public byte Amount { get; set; }

		public EODirection Direction { get; set; }
	}
}