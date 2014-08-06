using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EOLib;

namespace EndlessClient
{
	public class EOMapRenderer
	{
		public List<MapItem> MapItems { get; set; }
		public List<Character> OtherPlayers { get; set; }
		public List<NPC> NPCs { get; set; }

		public MapFile MapRef { get; private set; }

		public EOMapRenderer(MapFile mapObj)
		{
			MapRef = mapObj;
			MapItems = new List<MapItem>();
			OtherPlayers = new List<Character>();
			NPCs = new List<NPC>();
		}
	}
}
