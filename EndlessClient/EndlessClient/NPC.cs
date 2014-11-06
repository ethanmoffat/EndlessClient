using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EOLib;
using EOLib.Data;

namespace EndlessClient
{
	public class NPC
	{
		public byte Index { get; set; }
		public byte X { get; set; }
		public byte Y { get; set; }
		public EODirection Direction { get; set; }
		public NPCRecord Data { get; set; }

		public NPC(short ID)
		{
			Data = World.Instance.ENF.Data[ID] as NPCRecord;
		}

		public NPC(byte index, short id, byte x, byte y, EODirection dir)
		{
			Data = World.Instance.ENF.Data[id] as NPCRecord;

			Index = index;
			X = x;
			Y = y;
			Direction = dir;
		}

		public NPC(Packet pkt)
		{
			Index = pkt.GetChar();
			short id = pkt.GetShort();
			Data = World.Instance.ENF.Data[id] as NPCRecord;
			X = pkt.GetChar();
			Y = pkt.GetChar();
			Direction = (EODirection) pkt.GetChar();
		}
	}
}
